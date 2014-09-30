Imports System.Text.RegularExpressions

Public Class STD

    Public Shared Function ARGFilter(ByVal str As String, ByVal ParamArray pChars As Char()) As Dictionary(Of String, String)
        Dim out As New Dictionary(Of String, String)
        If Not str.StartsWith(" ") Then str = " " & str
        For iChr As Int32 = 0 To pChars.Length - 1
            Dim pchrx As String = "-" & pChars(iChr) & " "
            If Not str.Contains(" " & pchrx) Then out.Add(pchrx.TrimEnd, "") : Continue For
            Dim sspf As New List(Of String)(Split(str, pchrx))
            Dim argStr As String = sspf(1)
            Dim rxbase As String = New String(pChars).Replace(pChars(iChr), "")
            rxbase = ".*?(?=\s-[" & rxbase & "])"
            If Regex.IsMatch(argStr, "\s-[" & New String(pChars) & "]") Then
                argStr = Regex.Match(argStr, rxbase, RegexOptions.IgnoreCase).Value
            End If
            argStr = argStr.TrimEnd(" ").TrimEnd(" ")
            out.Add(pchrx.TrimEnd(" "), argStr)
        Next : Return out
    End Function


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
