Namespace Extraction

    Friend Class BitHelper
        Public Shared Function CopyBlock(bytes As Byte(), offset As Integer, length As Integer) As Byte()
            Dim startByte As Integer = offset / 8
            Dim endByte As Integer = (offset + length - 1) / 8
            Dim shiftA As Integer = offset Mod 8
            Dim shiftB As Integer = 8 - shiftA
            Dim dst As Byte() = New Byte((length + 7) / 8) {}

            If shiftA = 0 Then
                Buffer.BlockCopy(bytes, startByte, dst, 0, dst.Length)
            Else
                Dim i As Integer

                i = 0
                While i < endByte - startByte
                    dst(i) = (bytes(startByte + i) << shiftA Or bytes(startByte + i + 1) >> shiftB)
                    System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)
                End While

                If i < dst.Length Then
                    dst(i) = (bytes(startByte + i) << shiftA)
                End If
            End If

            dst(dst.Length - 1) = dst(dst.Length - 1) And (&HFF << dst.Length * 8 - length)

            Return dst
        End Function

        Public Shared Sub CopyBytes(dst As Byte(), dstOffset As Integer, src As Byte())
            Buffer.BlockCopy(src, 0, dst, dstOffset, src.Length)
        End Sub

        Public Shared Function Read(ByRef x As ULong, length As Integer) As Integer
            Dim r As Integer = (x >> 64 - length)
            x <<= length
            Return r
        End Function

        Public Shared Function Read(bytes As Byte(), ByRef offset As Integer, length As Integer) As Integer
            Dim startByte As Integer = offset / 8
            Dim endByte As Integer = (offset + length - 1) / 8
            Dim skipBits As Integer = offset Mod 8
            Dim bits As ULong = 0

            For i As Int32 = 0 To Math.Min(endByte - startByte, 7)
                bits = bits Or bytes(startByte + i) << 56 - i * 8
            Next
            If skipBits <> 0 Then
                Read(bits, skipBits)
            End If

            offset += length

            Return Read(bits, length)
        End Function
        Public Const flagfield = &HFFFFFFFFFFFFFFFFUL

        Public Shared Sub Write(ByRef x As ULong, length As Integer, value As Integer)
            Dim mask As ULong = CULng(&HFFFFFFFFFFFFFFFF >> 64 - length)
            x = x << length Or value And mask
        End Sub
    End Class


End Namespace