﻿/* MidiUtils

LICENSE - The MIT License (MIT)

Copyright (c) 2013-2014 Tomona Nanase

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

/* 規格出典: about Standard MIDI format
 *          http://www2s.biglobe.ne.jp/~yyagi/material/smfspec.html
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MidiUtils.IO;

namespace MidiUtils.Sequencer
{
    /// <summary>
    /// MIDI に関する一連のイベントを格納したシーケンスを提供します。
    /// </summary>
    public class Sequence
    {
        #region -- Private Fields --
        private readonly List<Track> tracks;
        #endregion

        #region -- Public Properties --
        /// <summary>
        /// 読み込まれた SMF のフォーマットを取得します。このプロパティは 0 または 1 のみの値となります。
        /// </summary>
        public int Format { get; private set; }

        /// <summary>
        /// シーケンスに関連付けられた分解能値を取得します。
        /// </summary>
        public int Resolution { get; private set; }

        /// <summary>
        /// 格納されたイベントの中で最大のティック数を取得します。
        /// </summary>
        public long MaxTick { get; private set; }

        /// <summary>
        /// 格納されたイベントの総数を取得します。
        /// </summary>
        public int EventCount { get; private set; }

        /// <summary>
        /// シーケンスが持つトラックの列挙子を取得します。
        /// </summary>
        public IEnumerable<Track> Tracks { get { return tracks; } }

        public List<Event> Events { get; private set; } 

        /// <summary>
        /// ループが開始されると判定されたティックを取得します。
        /// </summary>
        public long LoopBeginTick { get; private set; }
        #endregion

        #region -- Constructors --
        /// <summary>
        /// ファイルを指定して新しい Sequence クラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="filename">読み込まれる SMF (MIDI) ファイル名。</param>
        public Sequence(string filename)
        {
            if (String.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException();

            if (!File.Exists(filename))
                throw new FileNotFoundException();

            tracks = new List<Track>();

            using (Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                LoadFile(stream);
        }

        /// <summary>
        /// ストリームを指定して新しい Sequence クラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="stream">読み込み可能なストリーム。</param>
        public Sequence(Stream stream)
        {
            tracks = new List<Track>();
            LoadFile(stream);
        }
        #endregion

        #region -- Private Methods --
        private void LoadFile(Stream stream)
        {
            using (var br = new BinaryReader(stream))
            {
                // ヘッダ読み取り
                // マシンはリトルエンディアンなのでバイト列を適宜反転
                // (SMFは ビッグエンディアン で記述されている)

                var magic = br.ReadUInt32().ToLittleEndian();
                var endOfStream = stream.Length - 1;

                // マジックナンバー: 4D 54 68 64 (MThd)
                if (magic == 0x52494646)
                    endOfStream = SeekForRiff(br);
                else if (magic != 0x4d546864)
                    throw new InvalidDataException();

                // ヘッダ長 (6バイトで固定)
                if (br.ReadInt32().ToLittleEndian() != 6)
                    throw new InvalidDataException();

                // フォーマット (0 or 1. 2 についてはサポート対象外)
                Format = br.ReadInt16().ToLittleEndian();
                if (Format != 0 && Format != 1)
                    throw new InvalidDataException();

                // トラック数
                int trackCount = br.ReadInt16().ToLittleEndian();

                // 時間単位 (正数、つまり分解能のみサポート)
                Resolution = br.ReadInt16().ToLittleEndian();
                if (Resolution < 1)
                    throw new InvalidDataException();

                //o
                Events = new List<Event>();

                // トラックの追加
                var trackNumber = 0;
                while (stream.Position < endOfStream)
                {
                    // マジックナンバー: 4d 54 72 6b (MTrk)
                    if (br.ReadUInt32().ToLittleEndian() == 0x4d54726b)
                    {
                        // Track クラスに処理を移す
                        // Override: memory eff.
                        //tracks.Add(new Track(trackNumber, br));
                        Events.AddRange(new Track(trackNumber, br).Events);
                    }
                    else
                    {
                        // トラックチャンクでないなら、長さ分だけスキップ
                        var length = br.ReadUInt32().ToLittleEndian();
                        stream.Seek(length, SeekOrigin.Current);
                    }

                    trackNumber++;
                }

                //o Edited:
                //if (!tracks.Any(t =>
                //{
                //    if (!t.Events.Any())
                //        return true;
                //    var e = t.Events.Last();
                //    return (e.Type == EventType.MetaEvent && ((MetaEvent)e).MetaType == MetaType.EndOfTrack);
                //}))
                //    throw new InvalidDataException();
                //add Events = new List<Event>(Tracks.SelectMany(t => t.Events).OrderBy(t => t.Tick));
                Events.Sort((x, y) => x.Tick.CompareTo(y.Tick));
                EventCount = Events.Count;
                MaxTick = Events.Last().Tick;
                LoopBeginTick = DetectLoopBegin();
            }
        }

        private long SeekForRiff(BinaryReader br)
        {
            // RMI フォーマットに対応するためのメソッド
            // RIFF がリトルエンディアンで、SMF がビッグエンディアン
            // エンディアンネスが混在している
            // 混在している。

            var riffLength = br.ReadUInt32();
            uint length;

            if (riffLength > br.BaseStream.Length)
                throw new InvalidDataException();

            // マジックナンバー: 52 4d 49 44 (RMID)
            if (br.ReadUInt32() == 0x524d4944)
                throw new InvalidDataException();

            // マジックナンバー: 64 61 74 61 (data)
            while (br.ReadUInt32().ToLittleEndian() != 0x64617461)
            {
                // データチャンクでないなら、長さ分だけスキップ
                length = br.ReadUInt32();
                br.BaseStream.Seek(length, SeekOrigin.Current);
            }

            length = br.ReadUInt32();

            // マジックナンバー: 4D 54 68 64 (MThd)
            if (br.ReadUInt32() == 0x4d546864)
                throw new InvalidDataException();

            return br.BaseStream.Position + length;
        }

        private long DetectLoopBegin()
        {
            var k = Events.OfType<MidiEvent>().LastOrDefault(e => e.Type == EventType.ControlChange && e.Data1 == 111);
            return (k == null) ? 0 : k.Tick;
        }
        #endregion
    }
}
