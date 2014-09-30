Imports ytDownloader.Extraction.AudioExtractors

Namespace Extraction.VideoParsers
    Public Interface IChannelExtractor
        Inherits IDisposable
        Sub ExtractStreams()
        Function GetAudioExtractor(param As UInt32) As AudioChannelExtractor
        Sub CloseOutput(disposing As Boolean)
    End Interface
End Namespace