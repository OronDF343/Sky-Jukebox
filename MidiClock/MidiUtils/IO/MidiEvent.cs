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

using System.IO;

namespace MidiUtils.IO
{
    /// <summary>
    /// MIDI の演奏に関わるイベントを提供します。
    /// </summary>
    public class MidiEvent : Event
    {
        #region -- Public Properties --
        /// <summary>
        /// 1つ目のパラメータを取得します。
        /// </summary>
        public int Data1 { get; private set; }

        /// <summary>
        /// 2つ目のパラメータを取得します。
        /// </summary>
        public int Data2 { get; private set; }

        /// <summary>
        /// 対象となるチャネル番号を取得します。
        /// </summary>
        public int Channel { get; private set; }
        #endregion

        #region -- Constructors --
        /// <summary>
        /// パラメータを指定して新しい MidiEvent クラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="deltaTime">デルタタイム。</param>
        /// <param name="tick">ティック位置。</param>
        /// <param name="type">イベントのタイプ。</param>
        /// <param name="channel">チャネル番号。</param>
        /// <param name="br">読み込まれるバイトリーダ。</param>
        internal MidiEvent(int deltaTime, long tick, EventType type, int channel, BinaryReader br)
            : base(deltaTime, tick)
        {
            Type = type;
            Channel = channel;

            Load(br);
        }

        /// <summary>
        /// パラメータを指定して新しい MidiEvent クラスのインスタンスを初期化します。 
        /// </summary>
        /// <param name="type">イベントのタイプ。</param>
        /// <param name="channel">チャネル番号。</param>
        /// <param name="data1">1つ目のパラメータ。</param>
        /// <param name="data2">2つ目のパラメータ。</param>
        public MidiEvent(EventType type, int channel, int data1, int data2)
            : base(0, 0)
        {
            Type = type;
            Channel = channel;
            Data1 = data1;
            Data2 = data2;
        }

        /// <summary>
        /// このインスタンスを表す文字列を取得します。
        /// </summary>
        /// <returns>このインスタンスを表す文字列。</returns>
        public override string ToString()
        {
            return string.Format("{0}, Channel={1}, Control={2}", Type, Channel, Data1);
        }
        #endregion

        #region -- Private Methods --
        private void Load(BinaryReader br)
        {
            Data1 = br.ReadByte();

            if (Type != EventType.ProgramChange && Type != EventType.ChannelPressure)
                Data2 = br.ReadByte();
        }
        #endregion
    }
}
