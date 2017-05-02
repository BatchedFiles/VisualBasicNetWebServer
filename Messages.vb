''' <summary>
''' ������ ������ �������
''' </summary>
''' <remarks></remarks>
Public Module Messages

	Private Const HttpErrorHead As String = "<?xml version=""1.0"" encoding=""utf-8""?><!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.1 plus MathML 2.0 plus SVG 1.1//EN"" ""http://www.w3.org/2002/04/xhtml-math-svg/xhtml-math-svg.dtd""[<!ENTITY nbsp ""&#160;"">]><html xmlns=""http://www.w3.org/1999/xhtml"" xml:lang=""en"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.w3.org/1999/xhtml http://www.w3.org/MarkUp/SCHEMA/xhtml11.xsd"" xmlns:svg=""http://www.w3.org/2000/svg"" xmlns:xlink=""http://www.w3.org/1999/xlink"">" & _
	 "<head><meta name=""viewport"" content=""width=device-width, initial-scale=1"" /><title>{0}</title></head>"
	Private Const HttpErrorBody As String = "<body>" & _
	  "<h1>{0} ������ � ���������� �{1}�</h1>" & _
	  "<h2>������ HTTP {2} � {3}</h2><p>{4}</p><p>�������� <a href=""/"">������� ��������</a> �����.</p></body></html>"

	Public Function FormatErrorMessageBody(ByVal statusCode As Integer, ByVal appName As String, ByVal ErrorText As String) As String
		Dim desc = HttpWorkerRequest.GetStatusDescription(statusCode)
		Return String.Format(HttpErrorHead, desc) & String.Format(HttpErrorBody, If(statusCode >= 500, "���������", "����������"), appName, statusCode.ToString, desc, ErrorText)
	End Function

End Module
