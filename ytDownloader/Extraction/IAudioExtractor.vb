
Namespace Extraction
    Public Interface IAudioExtractor
        Inherits IDisposable

        Property VideoStream As IO.FileStream

        '''<exception cref="AudioExtractionException">An error occured while writing the chunk.</exception>
        Sub WriteChunk(chunk As Byte(), timeStamp As UInt32)
    End Interface
End Namespace