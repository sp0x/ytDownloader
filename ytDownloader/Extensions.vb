Imports System.Runtime.CompilerServices
Imports System.Text

Namespace Extensions
    Public Module UrlExtensions
        ''' <summary>
        ''' Decodes URLs, which are encoded with the % hex type encoding, and + for spaces.
        ''' </summary>
        ''' <param name="url">The url to decode</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> _
        Public Function URLDecode(url As String) As String
            Dim tmpBuilder As New StringBuilder
            Dim CurChr As Integer = 1
            Do Until CurChr - 1 = Len(url)
                Select Case Mid(url, CurChr, 1)
                    Case "+"
                        tmpBuilder.Append(" ")
                    Case "%"
                        tmpBuilder.Append(Chr(Val("&h" & _
                           Mid(url, CurChr + 1, 2))))
                        CurChr += 2
                    Case Else
                        tmpBuilder.Append(Mid(url, CurChr, 1))
                End Select

                CurChr = CurChr + 1
            Loop
            URLDecode = tmpBuilder.ToString
        End Function
        <Extension> Public Function URLEncode(StringToEncode As String, Optional UsePlusRatherThanHexForSpace As Boolean = False) As String
            Dim tmpBuilder As New StringBuilder
            Dim CurChr As Integer = 1
            Do Until CurChr - 1 = Len(StringToEncode)
                Select Case Asc(Mid(StringToEncode, CurChr, 1))
                    Case 48 To 57, 65 To 90, 97 To 122
                        tmpBuilder.Append(Mid(StringToEncode, CurChr, 1))
                    Case 32
                        If UsePlusRatherThanHexForSpace Then
                            tmpBuilder.Append("+")
                        Else
                            tmpBuilder.Append("%" & Hex(32))
                        End If
                    Case Else
                        tmpBuilder.Append("%" & _
                             Format(Hex(Asc(Mid(StringToEncode, CurChr, 1))), "00"))
                End Select
                CurChr += 1
            Loop
            URLEncode = tmpBuilder.ToString
        End Function


    End Module
End Namespace