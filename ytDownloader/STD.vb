Public Class STD
    Public Shared Function sprintf(pattern As String, ParamArray vals As String())
        Return String.Format(pattern, vals)
    End Function
    Public Shared Function inlineHelper(Of T)(ByRef target As T, ByVal value As T) As T
        target = value
        Return value
    End Function
    Public Shared Function RShift(int As Int32, positions As Byte) As Int32
        Return int / (Math.Pow(2, positions))
    End Function
    Public Shared Function RShift(lng As Int64, positions As Byte) As Int64
        Return lng / (Math.Pow(2, positions))
    End Function
    Public Shared Function RShift(int As UInt32, positions As Byte) As UInt32
        Return int / (Math.Pow(2, positions))
    End Function
    Public Shared Function RShift(lng As UInt64, positions As Byte) As UInt64
        Return lng / (Math.Pow(2, positions))
    End Function
    Public Shared Function LShift(int As Int32, positions As Byte) As Int32
        Return int * (Math.Pow(2, positions))
    End Function
    Public Shared Function LShift(lng As Int64, positions As Byte) As Int64
        Return lng * (Math.Pow(2, positions))
    End Function
    Public Shared Function LShift(int As UInt32, positions As Byte) As UInt32
        Return int * (Math.Pow(2, positions))
    End Function
    Public Shared Function LShift(lng As UInt64, positions As Byte) As UInt64
        Return lng * (Math.Pow(2, positions))
    End Function



End Class
