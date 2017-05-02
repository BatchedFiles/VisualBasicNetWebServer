Public Class Request
	Inherits SimpleWorkerRequest

	Private Enum CompressType
		None
		Xml
		JavaScript
	End Enum

	Private m_State As ReadHeadersResult
	Private m_VirtualPath As String
	Private m_PhysicalPath As String
	Private m_Stream As Stream
	Private m_BodyBuffer As New MemoryStream
	Private m_XmlCompress As CompressType

	Public Sub New(ByRef state As ReadHeadersResult, ByVal objStream As Stream, ByVal VirtualPath As String, ByVal PhysicalPath As String)
		MyBase.New(String.Empty, String.Empty, Nothing)
		m_State = state
		m_VirtualPath = VirtualPath
		m_PhysicalPath = PhysicalPath
		m_Stream = objStream
	End Sub

#Region "Реализация HttpWorkerRequest"

	Public Overrides Function GetUriPath() As String
		Return m_State.Path
	End Function

	Public Overrides Function GetQueryString() As String
		Return m_State.QueryString
	End Function

	'Public Overrides Function GetQueryStringRawBytes() As Byte()
	'	Return m_QueryStringBytes
	'End Function

	Public Overrides Function GetRawUrl() As String
		Return m_State.Url
	End Function

	Public Overrides Function GetHttpVerbName() As String
		Return m_State.GetHttpMethodString
	End Function

	Public Overrides Function GetHttpVersion() As String
		Select Case m_State.HttpVersion
			Case ReadHeadersResult.HttpVersions.Http11
				Return "HTTP/1.1"
			Case ReadHeadersResult.HttpVersions.Http10
				Return "HTTP/1.0"
			Case ReadHeadersResult.HttpVersions.Http09
				Return "HTTP/0.9"
			Case Else
				Return "HTTP/0.9"
		End Select
	End Function

	Public Overrides Function GetRemoteAddress() As String
		Return m_State.RemoteIP
	End Function

	Public Overrides Function GetRemotePort() As Integer
		Return 0
	End Function

	Public Overrides Function GetLocalAddress() As String
		Return m_State.LocalIp
	End Function

	Public Overrides Function GetLocalPort() As Integer
		Return m_State.LocalPort
	End Function

	Public Overrides Function GetFilePath() As String
		Return m_State.FilePath
	End Function

	Public Overrides Function GetFilePathTranslated() As String
		Return m_State.PathTranslated
	End Function

	Public Overrides Function GetPathInfo() As String
		' Это свойство не поддерживается, так как в каталогах может быть точка
		Return String.Empty
	End Function

	Public Overrides Function GetAppPath() As String
		Return m_VirtualPath
	End Function

	Public Overrides Function GetAppPathTranslated() As String
		Return m_PhysicalPath
	End Function

	Public Overrides Function GetPreloadedEntityBody() As Byte()
		Dim PreloadedContentLength As Integer = m_State.HeaderBytesLength - m_State.EndHeadersOffset
		If PreloadedContentLength > 0 Then
			Dim PreloadedContent(PreloadedContentLength - 1) As Byte
			Buffer.BlockCopy(m_State.HeaderBytes, m_State.EndHeadersOffset, PreloadedContent, 0, PreloadedContentLength)
			Return PreloadedContent
		Else
			Return Nothing
		End If
	End Function

	Public Overrides Function IsEntireEntityBodyIsPreloaded() As Boolean
		Return m_State.HeaderBytesLength - m_State.EndHeadersOffset > 0
	End Function

	Public Overrides Function ReadEntityBody(ByVal buffer() As Byte, ByVal size As Integer) As Integer
		Dim bytesRead As Integer
		Try
			bytesRead = m_Stream.Read(buffer, 0, size)
		Catch ex As Exception
			Return 0
		End Try
		Return bytesRead
	End Function

	Public Overrides Function GetKnownRequestHeader(ByVal index As Integer) As String
		Return m_State.RequestHeaders(index)
	End Function

	Public Overrides Function GetUnknownRequestHeader(ByVal name As String) As String
		For i As Integer = 0 To m_State.UnknownRequestHeadersCount - 1
			If m_State.UnknownRequestHeadersName(i) = name Then
				Return m_State.UnknownRequestHeadersValue(i)
			End If
		Next
		Return String.Empty
	End Function

	Public Overloads Overrides Function GetServerName() As String
		Return m_State.ServerName
	End Function

	Public Overrides Function GetServerVariable(ByVal name As String) As String
		Select Case name
			Case "ALL_RAW"
				Return m_State.AllRawRequestHeaders
			Case "SERVER_PROTOCOL"
				Return GetHttpVersion()
			Case Else
				Return String.Empty
		End Select
	End Function

	Public Overrides Function MapPath(ByVal path As String) As String
		Return m_State.MapPath(m_VirtualPath, path, m_PhysicalPath)
	End Function

	Public Overrides Sub SendStatus(ByVal statusCode As Integer, ByVal statusDescription As String)
		m_State.StatusCode = statusCode
		If statusCode >= 400 Then
			m_State.KeepAlive = False
		End If
	End Sub

	Public Overrides Sub SendKnownResponseHeader(ByVal index As Integer, ByVal value As String)
		If Not m_State.HeadersSent Then
			Select Case index
				Case HeaderConnection, HeaderDate
					' Игнорировать эти заголовки
					Return
				Case HeaderContentType
					Dim strContentType = value.Split(";"c)(0)
					Select Case strContentType
						Case "application/x-javascript"
							' Заменить заголовок устаревшего типа
							value = "application/javascript; charset=utf-8"
							strContentType = "application/javascript"
							m_XmlCompress = CompressType.JavaScript
						Case "application/javascript"
							value = "application/javascript; charset=utf-8"
							m_XmlCompress = CompressType.JavaScript
						Case "application/xhtml+xml", "text/html"
							' Необходимо применить сжатие
							' Удаление лишних пробелов, переносов строк и табуляций
							m_XmlCompress = CompressType.Xml
					End Select
					' Проверить на текстовое содержимое, чтобы можно было включать сжатие
					Dim kvp = m_State.ContentTypes.Find(Function(x) x.ContentType = strContentType)
					m_State.AddCompressionMethodHeader(If(kvp Is Nothing, False, kvp.IsTextFormat))
			End Select
			m_State.ResponseHeaders(index) = value
		End If
	End Sub

	Public Overrides Sub SendUnknownResponseHeader(ByVal name As String, ByVal value As String)
		If Not m_State.HeadersSent Then
			m_State.UnknownResponseHeaders.AppendLine(name & ": " & value)
		End If
	End Sub

	Public Overrides Sub SendCalculatedContentLength(ByVal contentLength As Integer)
		' Ничего не делаем
	End Sub

	Public Overrides Function HeadersSent() As Boolean
		Return m_State.HeadersSent
	End Function

	Public Overrides Function IsClientConnected() As Boolean
		Return True
	End Function

	Public Overrides Sub CloseConnection()

	End Sub

	''' <summary>
	''' Добавление данных в поток из *.aspx файлов
	''' </summary>
	Public Overloads Overrides Sub SendResponseFromMemory(ByVal data() As Byte, ByVal length As Integer)
		If length > 0 Then
			m_BodyBuffer.Write(data, 0, length)
		End If
	End Sub

	Public Overloads Overrides Sub SendResponseFromFile(ByVal filename As String, ByVal offset As Long, ByVal length As Long)
		If length > 0 Then
			Dim f As New FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)
			SendResponseFromFileStream(f, offset, length)
			f.Close()
		End If
	End Sub

	Public Overloads Overrides Sub SendResponseFromFile(ByVal handle As IntPtr, ByVal offset As Long, ByVal length As Long)
		If length > 0 Then
			Dim sh As Microsoft.Win32.SafeHandles.SafeFileHandle = New Microsoft.Win32.SafeHandles.SafeFileHandle(handle, True)
			Dim f As New FileStream(sh, FileAccess.Read)
			SendResponseFromFileStream(f, offset, length)
			f.Close()
		End If
	End Sub

	Public Overrides Sub FlushResponse(ByVal finalFlush As Boolean)
		Select Case m_XmlCompress
			Case CompressType.Xml
				' Сжать содержимое
				m_BodyBuffer.Seek(0, SeekOrigin.Begin)
				Using ms As New MemoryStream ' сжатое содержимое
					Using sr As New StreamReader(m_BodyBuffer) ' читатель
						Dim strLine = sr.ReadLine
						Do Until strLine Is Nothing
							' Очистить строку от пробелов и табуляций
							strLine = strLine.Trim(" "c, "	"c)
							' Получить байты строки
							Dim bytes = Encoding.UTF8.GetBytes(If(strLine.EndsWith("//<![CDATA["), strLine & vbCrLf, strLine))
							' Записать
							ms.Write(bytes, 0, bytes.Length)
							strLine = sr.ReadLine
						Loop
					End Using
					' Пересоздать поток ответа клиенту
					m_BodyBuffer.Close()
					m_BodyBuffer = New MemoryStream
					' Записать сжатое содержимое в поток ответа клиенту
					ms.Seek(0, SeekOrigin.Begin)
					ms.CopyTo(m_BodyBuffer)
				End Using
			Case CompressType.JavaScript
				' Сжать содержимое
				m_BodyBuffer.Seek(0, SeekOrigin.Begin)
				Using ms As New MemoryStream ' сжатое содержимое
					Using sr As New StreamReader(m_BodyBuffer) ' читатель
						Dim strLine = sr.ReadLine
						Do Until strLine Is Nothing
							' Очистить строку от пробелов и табуляций
							strLine = strLine.Trim(" "c, "	"c)
							' Получить байты строки
							Dim bytes = Encoding.UTF8.GetBytes(If(strLine.StartsWith("//"), String.Empty, If(strLine = "xmlRequestFrame.style.top = ""-100px""", "xmlRequestFrame.style.top = ""-100px"";", strLine & " ")))
							' Записать
							ms.Write(bytes, 0, bytes.Length)
							strLine = sr.ReadLine
						Loop
					End Using
					' Пересоздать поток ответа клиенту
					m_BodyBuffer.Close()
					m_BodyBuffer = New MemoryStream
					' Записать сжатое содержимое в поток ответа клиенту
					ms.Seek(0, SeekOrigin.Begin)
					ms.CopyTo(m_BodyBuffer)
				End Using
		End Select

		' Здесь можно сделать грязный хак
		' чтобы кешировать ресурсы типа axd

		If Not m_State.HeadersSent Then
			m_State.WriteHeaders(m_Stream, m_BodyBuffer.Length)
			m_State.HeadersSent = True
		End If
		If finalFlush Then
			m_BodyBuffer.Seek(0, SeekOrigin.Begin)
			If m_State.StatusCode >= 500 Then
				' Нужно протоколировать ошибки сервера
				ClientConnection.WriteLog(m_State.LockObject, m_State.Logs, Encoding.UTF8.GetString(m_BodyBuffer.ToArray))
				m_BodyBuffer.Seek(0, SeekOrigin.Begin)
			End If
			m_State.CopyBodyStreamToClientStream(m_Stream, m_BodyBuffer)
			m_State.Flush(m_Stream)
			m_BodyBuffer.Close()
			m_BodyBuffer.Dispose()
		End If
	End Sub

	Public Overrides Sub EndOfRequest()

	End Sub

	' проходит ли соединение через SSL
	Public Overrides Function IsSecure() As Boolean
		Return m_State.IsSecure
	End Function

	' Также нужно реализовать
	' GetClientCertificateValidUntil
	' GetClientCertificateValidFrom
	' GetClientCertificatePublicKey
	' GetClientCertificateEncoding
	' GetClientCertificateBinaryIssuer
	' GetClientCertificate

#End Region

	Private Sub SendResponseFromFileStream(ByVal f As FileStream, ByVal offset As Long, ByVal length As Long)
		Const maxChunkLength As Integer = 64 * 1024
		Dim fileSize As Long = f.Length

		If length = -1 Then
			length = fileSize - offset
		End If
		If length = 0 OrElse offset < 0 OrElse length > fileSize - offset Then
			Return
		End If
		If offset > 0 Then
			f.Seek(offset, SeekOrigin.Begin)
		End If
		If length <= maxChunkLength Then
			Dim fileBytes(CInt(length) - 1) As Byte
			Dim bytesRead As Integer = f.Read(fileBytes, 0, CInt(length))
			SendResponseFromMemory(fileBytes, bytesRead)
		Else
			Dim chunk(maxChunkLength - 1) As Byte
			Dim bytesRemaining As Integer = CInt(length)

			Do While bytesRemaining > 0
				Dim bytesToRead As Integer
				If bytesRemaining < maxChunkLength Then
					bytesToRead = bytesRemaining
				Else
					bytesToRead = maxChunkLength
				End If

				Dim bytesRead As Integer = f.Read(chunk, 0, bytesToRead)
				SendResponseFromMemory(chunk, bytesRead)
				bytesRemaining -= bytesRead

				' Сбросить все данные в поток
				If bytesRemaining > 0 AndAlso bytesRead > 0 Then
					FlushResponse(False)
				End If
			Loop
		End If
	End Sub

End Class
