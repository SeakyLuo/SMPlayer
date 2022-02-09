using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace SMPlayer
{
    public static class MusicHelper
    {
        public static List<string> SupportedFileTypes = new List<string>()
        {
            ".mp3", ".flac", ".aac", ".alac", ".wma"
        };

        public static void ReadTags(TagLib.File.IFileAbstraction fileAbstraction)
        {
            using (var tagFile = TagLib.File.Create(fileAbstraction, TagLib.ReadStyle.Average))
            {
                //read the raw tags
                var tags = tagFile.GetTag(TagLib.TagTypes.Id3v2, true);

                // do stuff with the tags 
            }
        }
    }

    public class MusicFileAbstraction : TagLib.File.IFileAbstraction
    {
        private readonly StorageFile file;

        public string Name => file.Name;

        public Stream ReadStream
        {
            get
            {
#pragma warning disable DF0023 // Disposing of this is handled by the TagLibSharp lib by calling the CloseStream method defined here
                return file.OpenStreamForReadAsync().GetAwaiter().GetResult();
#pragma warning restore DF0023 // Disposing of this is handled by the TagLibSharp libC:\Users\Seaky\source\repos\SMPlayer\SMPlayer\Helpers\MusicHelper.cs by calling the CloseStream method defined here
            }
        }

        public Stream WriteStream
        {
            get
            {

#pragma warning disable DF0023 // Disposing of this is handled by the TagLibSharp lib by calling the CloseStream method defined here
                return file.OpenStreamForWriteAsync().GetAwaiter().GetResult();
#pragma warning restore DF0023 // Disposing of this is handled by the TagLibSharp lib by calling the CloseStream method defined here
            }
        }


        public MusicFileAbstraction(StorageFile file)
        {
            this.file = file ?? throw new ArgumentNullException(nameof(file));
        }


        public void CloseStream(Stream stream)
        {
            stream?.Dispose();
        }
    }

}
