Imports System.Globalization
Imports System.Text
Imports System.IO
Namespace Extraction
    Public Class HtmlDecoder
        Private Shared ReadOnly s_entityEndingChars As Char() = New Char() {";"c, "&"c}
        Public Shared Function HtmlDecoder(s As String) As String
            If s Is Nothing Then
                Return Nothing
            End If
            If s.IndexOf("&"c) < 0 Then
                Return s
            End If
            Dim sb As New StringBuilder()
            Dim output As New StringWriter(sb)
            HtmlDecode(s, output)
            Return sb.ToString()
        End Function
        



        Public Shared Sub HtmlDecode(s As String, output As TextWriter)
            If Not s Is Nothing Then
                If s.IndexOf("&"c) < 0 Then
                    output.Write(s)
                Else
                    Dim length As Integer = s.Length
                    Dim i As Integer = 0
                    While i < length
                        Dim ch As Char = s(i)
                        If ch = "&"c Then ''obicham te  pandi''
                            Dim num3 As Integer = s.IndexOfAny(s_entityEndingChars, i + 1)
                            If (num3 > 0) AndAlso (s(num3) = ";"c) Then
                                Dim entity As String = s.Substring(i + 1, (num3 - i) - 1)
                                If (entity.Length > 1) AndAlso (entity(0) = "#"c) Then
                                    Try
                                        If (entity(1) = "x"c) OrElse (entity(1) = "X"c) Then
                                            ch = ChrW(Int32.Parse(entity.Substring(2), NumberStyles.AllowHexSpecifier))
                                        Else
                                            ch = ChrW(Int32.Parse(entity.Substring(1)))
                                        End If
                                        i = num3
                                    Catch generatedExceptionName As FormatException
                                        System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)
                                    Catch generatedExceptionName As ArgumentException
                                        System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)
                                    End Try
                                Else
                                    i = num3
                                    Dim ch2 As Char = HtmlEntities.Lookup(entity)
                                    If ch2 <> ControlChars.NullChar Then
                                        ch = ch2
                                    Else
                                        output.Write("&"c)
                                        output.Write(entity)
                                        output.Write(";"c)
                                        GoTo Label_0103
                                    End If
                                End If
                            End If
                        End If
                        output.Write(ch)
Label_0103:


                        System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)
                    End While
                End If
            End If
        End Sub
        Public Shared Function Decode(s As String) As String
            Dim strB As New StringBuilder()
            Using strWr As New StringWriter(strB)
                HtmlDecode(s, strWr)
            End Using
            Return strB.ToString
        End Function
        Private Class HtmlEntities
            Private Shared ReadOnly _entitiesList As String() = New String() {"""-quot", "&-amp", "<-lt", ">-gt", " -nbsp", "¡-iexcl", _
              "¢-cent", "£-pound", "¤-curren", "¥-yen", "¦-brvbar", "§-sect", _
              "¨-uml", "©-copy", "ª-ordf", "«-laquo", "¬-not", "­-shy", _
              "®-reg", "¯-macr", "°-deg", "±-plusmn", "²-sup2", "³-sup3", _
              "´-acute", "µ-micro", "¶-para", "·-middot", "¸-cedil", "¹-sup1", _
              "º-ordm", "»-raquo", "¼-frac14", "½-frac12", "¾-frac34", "¿-iquest", _
              "À-Agrave", "Á-Aacute", "Â-Acirc", "Ã-Atilde", "Ä-Auml", "Å-Aring", _
              "Æ-AElig", "Ç-Ccedil", "È-Egrave", "É-Eacute", "Ê-Ecirc", "Ë-Euml", _
              "Ì-Igrave", "Í-Iacute", "Î-Icirc", "Ï-Iuml", "Ð-ETH", "Ñ-Ntilde", _
              "Ò-Ograve", "Ó-Oacute", "Ô-Ocirc", "Õ-Otilde", "Ö-Ouml", "×-times", _
              "Ø-Oslash", "Ù-Ugrave", "Ú-Uacute", "Û-Ucirc", "Ü-Uuml", "Ý-Yacute", _
              "Þ-THORN", "ß-szlig", "à-agrave", "á-aacute", "â-acirc", "ã-atilde", _
              "ä-auml", "å-aring", "æ-aelig", "ç-ccedil", "è-egrave", "é-eacute", _
              "ê-ecirc", "ë-euml", "ì-igrave", "í-iacute", "î-icirc", "ï-iuml", _
              "ð-eth", "ñ-ntilde", "ò-ograve", "ó-oacute", "ô-ocirc", "õ-otilde", _
              "ö-ouml", "÷-divide", "ø-oslash", "ù-ugrave", "ú-uacute", "û-ucirc", _
              "ü-uuml", "ý-yacute", "þ-thorn", "ÿ-yuml", "Œ-OElig", "œ-oelig", _
              "Š-Scaron", "š-scaron", "Ÿ-Yuml", "ƒ-fnof", "ˆ-circ", "˜-tilde", _
              "Α-Alpha", "Β-Beta", "Γ-Gamma", "Δ-Delta", "Ε-Epsilon", "Ζ-Zeta", _
              "Η-Eta", "Θ-Theta", "Ι-Iota", "Κ-Kappa", "Λ-Lambda", "Μ-Mu", _
              "Ν-Nu", "Ξ-Xi", "Ο-Omicron", "Π-Pi", "Ρ-Rho", "Σ-Sigma", _
              "Τ-Tau", "Υ-Upsilon", "Φ-Phi", "Χ-Chi", "Ψ-Psi", "Ω-Omega", _
              "α-alpha", "β-beta", "γ-gamma", "δ-delta", "ε-epsilon", "ζ-zeta", _
              "η-eta", "θ-theta", "ι-iota", "κ-kappa", "λ-lambda", "μ-mu", _
              "ν-nu", "ξ-xi", "ο-omicron", "π-pi", "ρ-rho", "ς-sigmaf", _
              "σ-sigma", "τ-tau", "υ-upsilon", "φ-phi", "χ-chi", "ψ-psi", _
              "ω-omega", "ϑ-thetasym", "ϒ-upsih", "ϖ-piv", " -ensp", " -emsp", _
              " -thinsp", "‌-zwnj", "‍-zwj", "‎-lrm", "‏-rlm", "–-ndash", _
              "—-mdash", "‘-lsquo", "’-rsquo", "‚-sbquo", """-ldquo", """-rdquo", _
              "„-bdquo", "†-dagger", "‡-Dagger", "•-bull", "…-hellip", "‰-permil", _
              "′-prime", "″-Prime", "‹-lsaquo", "›-rsaquo", "‾-oline", "⁄-frasl", _
              "€-euro", "ℑ-image", "℘-weierp", "ℜ-real", "™-trade", "ℵ-alefsym", _
              "←-larr", "↑-uarr", "→-rarr", "↓-darr", "↔-harr", "↵-crarr", _
              "⇐-lArr", "⇑-uArr", "⇒-rArr", "⇓-dArr", "⇔-hArr", "∀-forall", _
              "∂-part", "∃-exist", "∅-empty", "∇-nabla", "∈-isin", "∉-notin", _
              "∋-ni", "∏-prod", "∑-sum", "−-minus", "∗-lowast", "√-radic", _
              "∝-prop", "∞-infin", "∠-ang", "∧-and", "∨-or", "∩-cap", _
              "∪-cup", "∫-int", "∴-there4", "∼-sim", "≅-cong", "≈-asymp", _
              "≠-ne", "≡-equiv", "≤-le", "≥-ge", "⊂-sub", "⊃-sup", _
              "⊄-nsub", "⊆-sube", "⊇-supe", "⊕-oplus", "⊗-otimes", "⊥-perp", _
              "⋅-sdot", "⌈-lceil", "⌉-rceil", "⌊-lfloor", "⌋-rfloor", "〈-lang", _
              "〉-rang", "◊-loz", "♠-spades", "♣-clubs", "♥-hearts", "♦-diams"}

            Public Shared Function Lookup(entity As String) As Char
                Dim hashtable As New Hashtable()
                For Each str As String In _entitiesList
                    hashtable(str.Substring(2)) = str(0)
                Next

                Dim obj2 As Object = hashtable(entity)
                If Not obj2 Is Nothing Then
                    Return DirectCast(obj2, Char)
                End If
                Return ControlChars.NullChar
            End Function
        End Class

    End Class
End Namespace