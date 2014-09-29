Namespace Extraction
    Public MustInherit Class BitConverterEx
        ''' <summary>
        ''' Converts the specified double-precision floating point number to a 64-bit signed integer.
        ''' </summary>
        ''' <param name="d"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function DoubleToInt64Bits(d As Double) As Long
            Return BitConverter.DoubleToInt64Bits(d)
        End Function
        Public Overloads Function Equals(obja As Object, objb As Object) As Boolean
            Return obja.Equals(objb)
        End Function
        ''' <summary>
        ''' Returns the specified Boolean value as an array of bytes.
        ''' </summary>
        ''' <param name="bool"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetBytes(bool As Boolean) As Byte()
            Return BitConverter.GetBytes(bool)
        End Function
        Public Shared Function GetBytes(val As Char) As Byte()
            Return BitConverter.GetBytes(val)
        End Function
        Public Shared Function GetBytes(val As Double) As Byte()
            Return BitConverter.GetBytes(val)
        End Function
        Public Shared Function GetBytes(val As Integer) As Byte()
            Return BitConverter.GetBytes(val)
        End Function
        Public Shared Function GetBytes(val As Long) As Byte()
            Return BitConverter.GetBytes(val)
        End Function
        Public Shared Function GetBytes(val As Short) As Byte()
            Return BitConverter.GetBytes(val)
        End Function
        Public Shared Function GetBytes(val As Single) As Byte()
            Return BitConverter.GetBytes(val)
        End Function
        Public Shared Function GetBytes(val As UInteger) As Byte()
            Return BitConverter.GetBytes(val)
        End Function
        Public Shared Function GetBytes(val As ULong) As Byte()
            Return BitConverter.GetBytes(val)
        End Function
        Public Shared Function GetBytes(val As UShort) As Byte()
            Return BitConverter.GetBytes(val)
        End Function
        Public Shared Function Int64BitsToDouble(lng As Long) As Double
            Return BitConverter.Int64BitsToDouble(lng)
        End Function
        Public Shared Function ToBoolean(bt() As Byte, int As Integer)
            Return BitConverter.ToBoolean(bt, int)
        End Function
        Public Shared Function ToChar(b() As Byte, int As Integer) As Char
            Return BitConverter.ToChar(b, int)
        End Function

        Public Shared Function ToDouble(b() As Byte, int As Int32) As Double
            Return BitConverter.ToDouble(b, int)
        End Function

        Public Shared Function ToUInt16(b() As Byte, int As Int32) As UInt16
            Return BitConverter.ToUInt16(b, int)
        End Function

        Public Shared Function ToUInt32(b() As Byte, int As Int32) As UInt32
            Return BitConverter.ToUInt32(b, int)
        End Function

        Public Shared Function ToUInt64(b() As Byte, int As Int32) As UInt64
            Return BitConverter.ToUInt64(b, int)
        End Function

        Public Shared Function ToInt16(b() As Byte, int As Int32) As Int16
            Return BitConverter.ToInt16(b, int)
        End Function

        Public Shared Function ToInt32(b() As Byte, int As Int32) As Int32
            Return BitConverter.ToInt32(b, int)
        End Function

        Public Shared Function ToInt64(b() As Byte, int As Int32) As Int64
            Return BitConverter.ToInt64(b, int)
        End Function

        Public Shared Function ToSingle(b() As Byte, int As Int32) As Single
            Return BitConverter.ToSingle(b, int)
        End Function

        Public Overloads Shared Function ToString(b() As Byte) As String
            Return BitConverter.ToString(b)
        End Function

        Public Overloads Shared Function ToString(b() As Byte, int As Int32) As String
            Return BitConverter.ToString(b, int)
        End Function
    End Class
   

    Public Class BigEndianBitConverter
        Inherits BitConverterEx
        Public Overloads Shared Function GetBytes(value As ULong) As Byte()
            Dim b() As Byte = BitConverter.GetBytes(value)
            Array.Reverse(b)
            Return b
        End Function
        Public Overloads Shared Function GetBytes(value As UInt32) As Byte()
            Dim b() As Byte = BitConverter.GetBytes(value)
            Array.Reverse(b)
            Return b
        End Function
        Public Overloads Shared Function GetBytes(value As UInt16) As Byte()
            Dim b() As Byte = BitConverter.GetBytes(value)
            Array.Reverse(b)
            Return b
        End Function
        Public Overloads Shared Function GetBytes(value As Int64) As Byte()
            Dim b() As Byte = BitConverter.GetBytes(value)
            Array.Reverse(b)
            Return b
        End Function
        Public Overloads Shared Function GetBytes(value As Int32) As Byte()
            Dim b() As Byte = BitConverter.GetBytes(value)
            Array.Reverse(b)
            Return b
        End Function
        Public Overloads Shared Function GetBytes(value As Int16) As Byte()
            Dim b() As Byte = BitConverter.GetBytes(value)
            Array.Reverse(b)
            Return b
        End Function
        Public Overloads Shared Function ToUInt16(vl As Byte(), Optional index As Int32 = 0) As UShort
            Array.Reverse(vl)
            Return BitConverter.ToUInt16(vl, index)
        End Function
        Public Overloads Shared Function ToUInt32(value As Byte(), startIndex As Integer) As UInteger
            Return CUInt(value(startIndex)) << 24 Or CUInt(value(startIndex + 1)) << 16 _
                Or CUInt(value(startIndex + 2)) << 8 Or value(startIndex + 3)
        End Function
        Public Overloads Shared Function ToUInt64(vl As Byte(), Optional index As Int32 = 0) As UShort
            Array.Reverse(vl)
            Return BitConverter.ToUInt64(vl, index)
        End Function
    End Class











    'Friend Class BigEndianBitConverter
    '    Public Shared Function ToUInt16(value As Byte(), startIndex As Integer) As UShort
    '        Return CUShort((value(startIndex) << 8 Or value(startIndex + 1)))
    '    End Function
    '    Public Shared Function ToUInt32(value As Byte(), startIndex As Integer) As UInteger
    '        Return CUInt(value(startIndex)) << 24 Or CUInt(value(startIndex + 1)) << 16 _
    '            Or CUInt(value(startIndex + 2)) << 8 Or value(startIndex + 3)
    '    End Function
    '    Public Shared Function ToUInt64(value As Byte(), startIndex As Integer) As ULong
    '        Return CULng(value(startIndex)) << 56 Or CULng(value(startIndex + 1)) << 48 _
    '            Or CULng(value(startIndex + 2)) << 40 Or CULng(value(startIndex + 3)) << 32 _
    '            Or CULng(value(startIndex + 4)) << 24 Or CULng(value(startIndex + 5)) << 16 _
    '            Or CULng(value(startIndex + 6)) << 8 Or value(startIndex + 7)
    '    End Function
    'End Class


End Namespace