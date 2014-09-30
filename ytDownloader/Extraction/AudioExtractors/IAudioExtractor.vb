
Namespace Extraction
    Public Interface IAudioExtractor
        Inherits IDisposable

        Property OutputStream As IO.Stream

        '''<exception cref="AudioExtractors.AudioExtractionException">An error occured while writing the chunk.</exception>
        Sub WriteChunk(chunk As Byte(), timeStamp As UInt32)

        Property OutputFile As String
    End Interface
End Namespace