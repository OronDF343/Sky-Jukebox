using System;
using FlacDotNet.Frames;
using FlacDotNet.Meta;

namespace FlacDotNet
{
    public interface IFrameListener
    {
        /**
     * Called for each Metadata frame read.
     * @param metadata The metadata frame read
     */
        void ProcessMetadata(Metadata metadata);

        /**
         * Called for each data frame read.
         * @param frame The data frame read
         */
        void ProcessFrame(ref Frame frame);

        /**
         * Called for each frame error detected.
         * @param msg   The error message
         */
        void ProcessError(String msg);
    }
}