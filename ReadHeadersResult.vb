''' <summary>
''' Результат чтения заголовков запроса
''' </summary>
<Serializable()> Public Structure ReadHeadersResult

	Public Class ByteRange
		Public Start As Long
		Public Count As Long
	End Class

	''' <summary>
	''' Версии протокола http
	''' </summary>
	Public Enum HttpVersions
		Http11
		Http10
		Http09
	End Enum

	''' <summary>
	''' Методы Http
	''' </summary>
	Public Enum HttpMethods
		None
		HttpGet
		HttpHead
		HttpPut
		HttpPatch
		HttpDelete
		HttpPost
		HttpOptions
		HttpTrace
		HttpCopy
		HttpMove
		HttpPropfind
	End Enum

	''' <summary>
	''' Флаги сжатия содержимого
	''' </summary>
	''' <remarks></remarks>
	Public Enum ZipModes
		''' <summary>
		''' Без сжатия
		''' </summary>
		''' <remarks></remarks>
		None
		''' <summary>
		''' Сжатие Deflate
		''' </summary>
		''' <remarks></remarks>
		Deflate
		''' <summary>
		''' Сжатие GZip
		''' </summary>
		''' <remarks></remarks>
		GZip
	End Enum

	''' <summary>
	''' Максимальное количество байт в заголовках клиента
	''' </summary>
	Public Const MaxRequestHeaderBytes As Integer = 32 * 1024

	''' <summary>
	''' Первая строка запроса клиента
	''' </summary>
	Public RequestLine As String

	''' <summary>
	''' Байты запроса клиента (заголовок + частично тело)
	''' </summary>
	Public HeaderBytes() As Byte

	''' <summary>
	''' Количество байт запроса клиента
	''' </summary>
	''' <remarks></remarks>
	Public HeaderBytesLength As Integer

	''' <summary>
	''' Индекс первого байта начала заголовков HTTP (метода, запрошенного ресурса и версии протокола)
	''' </summary>
	Public StartHeadersOffset As Integer

	''' <summary>
	''' Индекс первого байта после конца заголовков HTTP (конец заголовков + пустая строка)
	''' Указывает на начало тела запроса (если оно есть)
	''' </summary>
	Public EndHeadersOffset As Integer

	''' <summary>
	''' Версия http‐протокола
	''' </summary>
	Public HttpVersion As HttpVersions

	''' <summary>
	''' Метод HTTP
	''' </summary>
	Public HttpMethod As HttpMethods

	''' <summary>
	''' Запрошенный клиентом адрес
	''' </summary>
	Public Url As String

	''' <summary>
	''' Путь, указанный клиентом
	''' </summary>
	''' <remarks></remarks>
	Public Path As String
	''' <summary>
	''' Путь к файлу
	''' </summary>
	''' <remarks></remarks>
	Public FilePath As String
	''' <summary>
	''' Путь к файлу на диске
	''' </summary>
	''' <remarks></remarks>
	Public PathTranslated As String
	'' Это свойство не поддерживается, так как в каталогах может быть точка
	'Public PathInfo As String

	''' <summary>
	''' Строка запроса
	''' </summary>
	Public QueryString As String

	''' <summary>
	''' Код ответа клиенту
	''' </summary>
	Public StatusCode As Integer

	''' <summary>
	''' Отправлять клиенту только заголовки
	''' </summary>
	Public SendOnlyHeaders As Boolean

	''' <summary>
	''' Поддерживать соединение с клиентом
	''' </summary>
	Public KeepAlive As Boolean

	''' <summary>
	''' Длина тела запроса клиента
	''' </summary>
	Public RequestBodyContentLength As Integer

	''' <summary>
	''' Строка со всеми заголовками запроса
	''' </summary>
	Public AllRawRequestHeaders As String

	' ''' <summary>
	' ''' Нераспознанные заголовки запроса
	' ''' </summary>
	Public Const UnknownRequestHeadersMaximum As Integer = 20
	Public UnknownRequestHeadersName() As String
	Public UnknownRequestHeadersValue() As String
	Public UnknownRequestHeadersCount As Integer

	''' <summary>
	''' Распознанные заголовки запроса
	''' </summary>
	Public RequestHeaders() As String

	''' <summary>
	''' Заголовки отправлены клиенту
	''' </summary>
	Public HeadersSent As Boolean

	' ''' <summary>
	' ''' Заголовки ответа
	' ''' </summary>
	Public ResponseHeaders() As String

	''' <summary>
	''' Неизвестные заголовки ответа
	''' </summary>
	Public UnknownResponseHeaders As StringBuilder

	Public LocalIp As String
	Public RemoteIP As String
	Public LocalPort As Integer
	Public IsSecure As Boolean
	Public ServerName As String

	''' <summary>
	''' Миме‐типы файлов
	''' </summary>
	Public ContentTypes As List(Of ContentType)

	''' <summary>
	''' Ёмкость для потока в памяти 100 КиБ, вроде этого должно хватить всем
	''' </summary>
	Public Const MemoryStreamCapacity As Integer = 100 * 1024

	''' <summary>
	''' Сжатие данных
	''' </summary>
	Public ZipEnabled As ZipModes

	''' <summary>
	''' Поток в памяти для записи сжатых данных
	''' </summary>
	Public MemoryStream As MemoryStream

	''' <summary>
	''' Сжатый поток
	''' </summary>
	Public ZipStream As Stream

	Public LowerCasedVirtualPath As String
	Public LowerCasedVirtualPathWithTrailingSlash As String

	''' <summary>
	''' Объект блокировки и синхронизации
	''' </summary>
	''' <remarks></remarks>
	Public LockObject As Object

	''' <summary>
	''' Коллекция логов сервера
	''' </summary>
	''' <remarks></remarks>
	Public Logs As MemoryStream

	Public WebSites As Dictionary(Of String, WebSite)

	''' <summary>
	''' Добавляет заголовки компрессии gzip или deflate
	''' </summary>
	''' <remarks></remarks>
	Public Sub AddCompressionMethodHeader(ByVal IsText As Boolean)
		' Потоки сжатия будут создаваться только при текстовом содержимом
		If IsText Then
			If Not String.IsNullOrEmpty(RequestHeaders(HttpWorkerRequest.HeaderAcceptEncoding)) Then
				Const Deflate As String = "deflate"
				' Сжатие трафика
				If RequestHeaders(HttpWorkerRequest.HeaderAcceptEncoding).Contains(Deflate) Then
					ZipEnabled = ZipModes.Deflate
					ResponseHeaders(HttpWorkerRequest.HeaderContentEncoding) = Deflate
					MemoryStream = New MemoryStream(ReadHeadersResult.MemoryStreamCapacity)
					ZipStream = New IO.Compression.DeflateStream(MemoryStream, Compression.CompressionMode.Compress, True)
				Else
					Const Gzip As String = "gzip"
					If RequestHeaders(HttpWorkerRequest.HeaderAcceptEncoding).Contains(Gzip) Then
						ZipEnabled = ZipModes.GZip
						ResponseHeaders(HttpWorkerRequest.HeaderContentEncoding) = Gzip
						MemoryStream = New MemoryStream(ReadHeadersResult.MemoryStreamCapacity)
						ZipStream = New IO.Compression.GZipStream(MemoryStream, Compression.CompressionMode.Compress, True)
					End If
				End If
			End If
		Else
			ZipEnabled = ZipModes.None
			ResponseHeaders(HttpWorkerRequest.HeaderContentEncoding) = Nothing
		End If
	End Sub

	''' <summary>
	''' Добавляет заголовки кеширования для файла и проверяет совпадение на заголовки кэширования
	''' </summary>
	''' <remarks></remarks>
	Public Sub AddCacheHeaders()
		' Проверить ETag, если совпадает, вернуть, что не изменилось
		'If-None-Match: "686897696a7c876b7e"
		'304 Not Modified
		' Ещё для кеширования
		'Last-Modified и If-Modified-Since
		'304 Not Modified
		Dim f As New FileInfo(PathTranslated)
		' Дата последней модицикации и контрольная сумма
		Dim dFileLastModified As Date = f.LastWriteTimeUtc
		' ETag для сжатого и несжатого содержимого должен быть разным
		Dim ETag As String = If(f.Exists, """" & f.LastWriteTime.ToBinary.ToString & If(ZipEnabled = ZipModes.GZip, "gzip""", If(ZipEnabled = ZipModes.Deflate, "deflate""", """")), String.Empty)

		Dim strClientDate = RequestHeaders(HttpWorkerRequest.HeaderIfModifiedSince)

		Dim bModified As Boolean = True
		If String.IsNullOrEmpty(strClientDate) Then
			bModified = True
		Else
			Dim ClientDate As Date
			' Убрать UTC и заменить на GMT
			'If-Modified-Since: Thu, 24 Mar 2016 16:10:31 UTC
			'If-Modified-Since: Tue, 11 Mar 2014 20:07:57 GMT
			Dim blnResult As Boolean = Date.TryParse(strClientDate.Replace("UTC", "GMT").Split(";"c)(0), ClientDate)
			If blnResult Then
				If ClientDate.ToLocalTime < New Date(dFileLastModified.Year, dFileLastModified.Month, dFileLastModified.Day, dFileLastModified.Hour, dFileLastModified.Minute, dFileLastModified.Second) Then
					bModified = True
				Else
					bModified = False
				End If
			Else
				bModified = True
			End If
		End If

		If bModified Then
			If Not String.IsNullOrEmpty(RequestHeaders(HttpWorkerRequest.HeaderIfNoneMatch)) Then
				bModified = (RequestHeaders(HttpWorkerRequest.HeaderIfNoneMatch) <> ETag)
			End If
		End If

		SendOnlyHeaders = SendOnlyHeaders OrElse Not bModified
		ResponseHeaders(HttpWorkerRequest.HeaderLastModified) = dFileLastModified.ToString("R", DateTimeFormatInfo.InvariantInfo)
		ResponseHeaders(HttpWorkerRequest.HeaderEtag) = ETag
		ResponseHeaders(HttpWorkerRequest.HeaderCacheControl) = "max-age=2678400" ' Целый месяц кэширования

		StatusCode = If(bModified, 200, 304)
	End Sub

	''' <summary>
	''' Добавляет заголовок в заголовки запроса клиента
	''' </summary>
	''' <param name="Header"></param>
	''' <param name="Value"></param>
	''' <returns>Возвращает True, если заголовок Host был пустым</returns>
	''' <remarks></remarks>
	Public Function AddRequestHeader(ByVal Header As String, ByVal Value As String) As Boolean
		Dim knownIndex As Integer = HttpWorkerRequest.GetKnownRequestHeaderIndex(Header)
		If knownIndex >= 0 Then
			' Дополнительные действия по заголовкам запроса
			Select Case knownIndex
				Case HttpWorkerRequest.HeaderConnection
					' Поддержка соединения
					If Value.ToLower.Contains("close") Then
						KeepAlive = False
					Else
						If Value.ToLower.Contains("keep-alive") Then
							KeepAlive = True
						End If
					End If
				Case HttpWorkerRequest.HeaderAcceptEncoding
				Case HttpWorkerRequest.HeaderHost
					' Заголовок Host, он же имя сайта
					If Value.Length = 0 Then
						Return True
					End If
			End Select
			RequestHeaders(knownIndex) = Value
		Else
			' Добавить в нераспознанные заголовки
			If UnknownRequestHeadersCount < UnknownRequestHeadersMaximum Then
				UnknownRequestHeadersName(UnknownRequestHeadersCount) = Header
				UnknownRequestHeadersValue(UnknownRequestHeadersCount) = Value
				UnknownRequestHeadersCount += 1
			End If
		End If
		Return False
	End Function

	''' <summary>
	''' Добавляет заголовок в заголовки ответа сервера
	''' </summary>
	''' <param name="Header"></param>
	''' <param name="Value"></param>
	''' <remarks></remarks>
	Public Sub AddResponseHeader(ByVal Header As String, ByVal Value As String)
		Dim knownIndex As Integer = HttpWorkerRequest.GetKnownResponseHeaderIndex(Header)
		If knownIndex >= 0 Then
			ResponseHeaders(knownIndex) = Value
		Else
			UnknownResponseHeaders.AppendLine(Header & ": " & Value)
		End If
	End Sub

	Public Function GetBytesRange(ByVal FileLength As Long) As List(Of ByteRange)
		' Нужно проверить частичный запрос
		Dim result As New List(Of ByteRange)
		If String.IsNullOrEmpty(RequestHeaders(HttpWorkerRequest.HeaderRange)) Then
			' Выдать всё содержимое от начала до конца
			result.Add(New ByteRange With {.Count = FileLength})
		Else
			' Выдать только диапазон
			If RequestHeaders(HttpWorkerRequest.HeaderRange).StartsWith("bytes=") Then
				Dim c As Integer = RequestHeaders(HttpWorkerRequest.HeaderRange).IndexOf("=")
				If c > 0 Then
					Dim value As String = RequestHeaders(HttpWorkerRequest.HeaderRange).Substring(c + 1)
					If value.Length > 0 Then
						'bytes=0-255 — фрагмент от 0-го до 255-го байта включительно.
						'bytes=42-42 — запрос одного 42-го байта.
						'bytes=4000-7499,1000-2999 — два фрагмента. Так как первый выходит за пределы, то он интерпретируется как «4000-4999».
						'bytes=3000-,6000-8055 — первый интерпретируется как «3000-4999», а второй игнорируется.
						'bytes=-400,-9000 — последние 400 байт (от 4600 до 4999), а второй подгоняется под рамки содержимого (от 0 до 4999) обозначая как фрагмент весь объём.
						'bytes=500-799,600-1023,800-849 — при пересечениях диапазоны могут объединяться в один (от 500 до 1023).
						For Each range As String In value.Split(","c)
							c = range.IndexOf("=")
							If c >= 0 Then
								' Начало
								Dim nStart As Long
								Long.TryParse(range.Substring(0, c), nStart)
								' Конец
								Dim nEnd As Long
								Long.TryParse(range.Substring(c + 1), nEnd)
							End If
						Next
					End If
				End If
			End If
		End If
		Return result
	End Function

	Public Sub SetHttpMethod(ByVal method As String)
		Select Case method
			Case "COPY"
				HttpMethod = HttpMethods.HttpCopy
			Case "DELETE"
				HttpMethod = HttpMethods.HttpDelete
			Case "GET"
				HttpMethod = HttpMethods.HttpGet
			Case "HEAD"
				HttpMethod = HttpMethods.HttpHead
			Case "MOVE"
				HttpMethod = HttpMethods.HttpMove
			Case "OPTIONS"
				HttpMethod = HttpMethods.HttpOptions
			Case "PATCH"
				HttpMethod = HttpMethods.HttpPatch
			Case "POST"
				HttpMethod = HttpMethods.HttpPost
			Case "PROPFIND"
				HttpMethod = HttpMethods.HttpPropfind
			Case "PUT"
				HttpMethod = HttpMethods.HttpPut
			Case "TRACE"
				HttpMethod = HttpMethods.HttpTrace
		End Select
	End Sub

	Public Function GetHttpMethodString() As String
		Select Case HttpMethod
			Case HttpMethods.HttpGet
				Return "GET"
			Case HttpMethods.HttpHead
				Return "HEAD"
			Case HttpMethods.HttpPut
				Return "PUT"
			Case HttpMethods.HttpPatch
				Return "PATCH"
			Case HttpMethods.HttpDelete
				Return "DELETE"
			Case HttpMethods.HttpPost
				Return "POST"
			Case HttpMethods.HttpOptions
				Return "OPTIONS"
			Case HttpMethods.HttpTrace
				Return "TRACE"
			Case HttpMethods.HttpCopy
				Return "COPY"
			Case HttpMethods.HttpMove
				Return "MOVE"
			Case HttpMethods.HttpPropfind
				Return "PROPFIND"
			Case Else
				Return String.Empty
		End Select
	End Function

	Public Function ReadLine(ByVal objStream As Stream) As String
		' Найти в буфере vbCrLf
		Dim FindIndex As Integer = FindCrLf(HeaderBytes, EndHeadersOffset, HeaderBytesLength)
		Dim line As String
		Do While FindIndex = -1
			' Символы переноса строки не найдены

			' Если буфер заполнен, то считывать данные больше нельзя
			If HeaderBytesLength >= ReadHeadersResult.MaxRequestHeaderBytes Then
				Return Nothing
			Else
				' Считать данные
				Dim numReceived As Integer
				Try
					numReceived = objStream.Read(HeaderBytes, HeaderBytesLength, ReadHeadersResult.MaxRequestHeaderBytes - HeaderBytesLength)
				Catch ex As Exception
					Return Nothing
				End Try
				If numReceived = 0 Then
					' Если в буфере что‐то было?
					If EndHeadersOffset < HeaderBytesLength Then
						line = Encoding.ASCII.GetString(HeaderBytes, EndHeadersOffset, HeaderBytesLength - EndHeadersOffset)
						' Поставить конец заголовков за пределы полученных байт
						EndHeadersOffset = HeaderBytesLength
						Return line
					Else
						Return Nothing
					End If
				Else
					' Сколько байт получили, на столько и увеличили буфер
					HeaderBytesLength += numReceived
					' Найти vbCrLf опять
					FindIndex = FindCrLf(HeaderBytes, EndHeadersOffset, HeaderBytesLength)
				End If
			End If
			Debug.WriteLine("Выполняю ReadLine")
		Loop

		' Найдено, получить строку
		line = Encoding.ASCII.GetString(HeaderBytes, EndHeadersOffset, FindIndex - EndHeadersOffset)
		' Сдвинуть конец заголовков вправо на FindIndex + len(vbCrLf)
		EndHeadersOffset = FindIndex + 2
		Return line
	End Function

	''' <summary>
	''' Читает запрос клиента
	''' </summary>
	''' <param name="objStream">Клиентский поток</param>
	''' <remarks></remarks>
	Public Function ReadAllHeaders(ByVal objStream As Stream) As ParseRequestLineResult
		' Проверить наличие данных от клиента
		RequestLine = ReadLine(objStream)
		If RequestLine Is Nothing Then
			Return ParseRequestLineResult.EmptyRequest
		End If

		' Метод, запрошенный ресурс и версия протокола
		Dim HttpMethods() As String = RequestLine.Split
		Select Case HttpMethods.Length
			Case 2
				HttpVersion = ReadHeadersResult.HttpVersions.Http09
			Case 3
				Select Case HttpMethods(2)
					Case "HTTP/1.0"
						HttpVersion = ReadHeadersResult.HttpVersions.Http10
					Case "HTTP/1.1"
						HttpVersion = ReadHeadersResult.HttpVersions.Http11
						KeepAlive = True ' Для версии 1.1 это по умолчанию
					Case Else
						' Протокол не поддерживается
						Return ParseRequestLineResult.HTTPVersionNotSupported
				End Select
			Case Else
				Return ParseRequestLineResult.BadRequest
		End Select

		' Проверить поддерживаемый метод
		SetHttpMethod(HttpMethods(0))
		If HttpMethod = ReadHeadersResult.HttpMethods.None Then
			Return ParseRequestLineResult.MethodNotSupported
		End If

		' Начало заголовков
		StartHeadersOffset = EndHeadersOffset
		' Отправлять только заголовки
		SendOnlyHeaders = (HttpMethod = ReadHeadersResult.HttpMethods.HttpHead)
		' Запрошенный клиентом ресурс
		Url = HttpMethods(1)

		' Получить все заголовки запроса
		Do
			Dim line As String = ReadLine(objStream)
			If line Is Nothing Then
				' Ошибка, строка не должна быть Nothing
				Return ParseRequestLineResult.EmptyRequest
			Else
				If line.Length = 0 Then
					' Клиент отправил все данные, можно приступать к обработке
					Exit Do
				Else
					'UNDONE Обработать ситуацию, когда клиент отправляет заголовок с переносом на новую строку
					Dim c As Integer = line.IndexOf(":"c)
					If c > 0 Then
						Dim name As String = line.Substring(0, c)
						Dim value As String = line.Substring(c + 1).Trim()

						If AddRequestHeader(name, value) Then
							Return ParseRequestLineResult.HostRequired
						End If
					End If
				End If
			End If
		Loop

		' Проверить заголовок Host
		If String.IsNullOrEmpty(RequestHeaders(HttpWorkerRequest.HeaderHost)) Then
			Return ParseRequestLineResult.HostRequired
		End If
		' Запомнить все заголовки в одну сплошную строку
		AllRawRequestHeaders = Encoding.ASCII.GetString(HeaderBytes, StartHeadersOffset, EndHeadersOffset - StartHeadersOffset)

		Return ParseRequestLineResult.Success
	End Function

	''' <summary>
	''' Разбор отправленного клиентом содержимого
	''' </summary>
	''' <remarks></remarks>
	Public Function ParsePostedContent() As ClientConnection.ParsePostedContentResult
		If String.IsNullOrEmpty(RequestHeaders(HttpWorkerRequest.HeaderContentLength)) Then
			' Требуется указание длины
			Return ParsePostedContentResult.LengthRequired
		End If

		' Длина содержимого по заголовку Content-Length
		If Integer.TryParse(RequestHeaders(HttpWorkerRequest.HeaderContentLength), RequestBodyContentLength) Then
			' Не больше четырёх метров
			If RequestBodyContentLength > 4194304 Then
				Return ParsePostedContentResult.RequestEntityTooLarge
			End If
			Return ParsePostedContentResult.None
		Else
			' Плохой запрос
			Return ParsePostedContentResult.BadRequest
		End If
	End Function

	''' <summary>
	''' Проверяет авторизацию Http
	''' </summary>
	''' <param name="objStream"></param>
	''' <param name="UsersFile"></param>
	''' <param name="VirtualPath"></param>
	''' <returns>Возвращает True, если авторизация не прошла</returns>
	''' <remarks></remarks>
	Public Function HttpAuth(ByVal objStream As Stream, ByVal UsersFile As String, ByVal VirtualPath As String) As Boolean
		' Проверка заголовка Authorization
		If String.IsNullOrEmpty(RequestHeaders(HttpWorkerRequest.HeaderAuthorization)) Then
			' Требуется авторизация
			StatusCode = 401
			ResponseHeaders(HttpWorkerRequest.HeaderWwwAuthenticate) = "Basic realm=""Need username and password"""
			WriteError(objStream, "Требуется логин и пароль для доступа", VirtualPath)
			Return True
		Else
			Dim sAuth = RequestHeaders(HttpWorkerRequest.HeaderAuthorization).Split
			If sAuth.Length > 1 Then
				If sAuth(0) = "Basic" Then
					' Расшифровать
					Dim UsernamePassword As String = Encoding.ASCII.GetString(Convert.FromBase64String(sAuth(1)))
					' Проверить имя пользователя и пароль
					'Dim NamePass() As String = UsernamePassword.Split(":c")
					Dim q = From s As String In File.ReadAllLines(UsersFile) Where s = UsernamePassword
					If q.Count = 0 Then
						' Ошибка 401
						' Имя пользователя и пароль не подходят
						StatusCode = 401
						ResponseHeaders(HttpWorkerRequest.HeaderWwwAuthenticate) = "Basic realm=""Need username and password"""
						WriteError(objStream, "Требуется логин и пароль для доступа", VirtualPath)
						Return True
					Else
						Return False
					End If
				Else
					StatusCode = 401
					ResponseHeaders(HttpWorkerRequest.HeaderWwwAuthenticate) = "Basic realm=""Use Basic auth"""
					WriteError(objStream, "Требуется Basic‐авторизация", VirtualPath)
					Return True
				End If
			Else
				StatusCode = 401
				ResponseHeaders(HttpWorkerRequest.HeaderWwwAuthenticate) = "Basic realm=""Authorization"""
				WriteError(objStream, "Параметры авторизации неверны", VirtualPath)
				Return True
			End If
		End If
	End Function

	''' <summary>
	''' Отправляет код статуса 100, чтобы клиент ждал обработку запроса
	''' </summary>
	Public Sub Write100Continue(ByVal objStream As Stream)
		Dim HeaderBytes = Encoding.ASCII.GetBytes(MakeResponseHeaders(0))
		WriteToClient(objStream, HeaderBytes, 0, HeaderBytes.Length)
	End Sub

	''' <summary>
	''' Записывает ошибку ответа в поток
	''' </summary>
	''' <remarks></remarks>
	Public Sub WriteError(ByVal objStream As Stream, ByVal strMessage As String, ByVal VirtualPath As String)
		AddCompressionMethodHeader(True)
		' Тип документа
		ResponseHeaders(HttpWorkerRequest.HeaderContentType) = "application/xhtml+xml; charset=utf-8"
		' Русский язык для страниц ошибок
		ResponseHeaders(HttpWorkerRequest.HeaderContentLanguage) = "ru-RU,ru"
		' Не поддерживаем соединение при ошибках
		KeepAlive = False
		
		Dim BodyBytes() As Byte = Encoding.UTF8.GetBytes(Messages.FormatErrorMessageBody(StatusCode, VirtualPath, strMessage))

		' Если включено сжатие, нужно записать байты тела в сжатый поток
		If ZipEnabled = ReadHeadersResult.ZipModes.None Then
			Dim HeaderBytes = Encoding.ASCII.GetBytes(MakeResponseHeaders(BodyBytes.Length))
			WriteToClient(objStream, HeaderBytes, 0, HeaderBytes.Length)
		End If
		WriteBody(objStream, BodyBytes, 0, BodyBytes.Length)
		Flush(objStream)
	End Sub

	''' <summary>
	''' Записывает заголовки ответа в поток
	''' </summary>
	''' <remarks></remarks>
	Public Sub WriteHeaders(ByVal objStream As Stream, ByVal ContentLength As Long)
		If ZipEnabled = ReadHeadersResult.ZipModes.None Then
			Dim bytes() As Byte = Encoding.ASCII.GetBytes(MakeResponseHeaders(ContentLength))
			WriteToClient(objStream, bytes, 0, bytes.Length)
			Erase bytes
		End If
	End Sub

	''' <summary>
	''' Записывает байты тела ответа в поток
	''' </summary>
	''' <param name="bytes"></param>
	''' <param name="offset"></param>
	''' <param name="length"></param>
	''' <remarks></remarks>
	Public Sub WriteBody(ByVal objStream As Stream, ByVal bytes() As Byte, ByVal offset As Integer, ByVal length As Integer)
		If ZipEnabled = ReadHeadersResult.ZipModes.None Then
			If Not SendOnlyHeaders Then
				Try
					objStream.Write(bytes, offset, length)
				Catch ex As Exception
				End Try
			End If
		Else
			ZipStream.Write(bytes, offset, length)
		End If
	End Sub

	''' <summary>
	''' Копирует байты тела ответа в клиентский поток
	''' </summary>
	''' <remarks></remarks>
	Public Sub CopyBodyStreamToClientStream(ByVal DestinationStream As Stream, ByVal SourceStream As Stream)
		If ZipEnabled = ReadHeadersResult.ZipModes.None Then
			Try
				SourceStream.CopyTo(DestinationStream)
			Catch ex As Exception
			End Try
		Else
			SourceStream.CopyTo(ZipStream)
		End If
	End Sub

	''' <summary>
	''' Сбрасывает данные в сетевой поток
	''' </summary>
	''' <remarks></remarks>
	Public Sub Flush(ByVal objStream As Stream)
		If ZipEnabled <> ReadHeadersResult.ZipModes.None Then
			ZipStream.Flush()
			ZipStream.Close()
			ZipStream = Nothing
			' Отправить заголовки
			Dim HeaderBytes = Encoding.ASCII.GetBytes(MakeResponseHeaders(MemoryStream.Length))
			WriteToClient(objStream, HeaderBytes, 0, HeaderBytes.Length)
			Erase HeaderBytes
			If Not SendOnlyHeaders Then
				' Отправить тело
				MemoryStream.Seek(0, SeekOrigin.Begin)
				Try
					MemoryStream.CopyTo(objStream)
				Catch ex As Exception
				End Try
			End If
			MemoryStream.Close()
			MemoryStream.Dispose()
			MemoryStream = Nothing
		End If
		Try
			objStream.Flush()
		Catch ex As Exception
		End Try
	End Sub

	''' <summary>
	''' Создаёт заголовки ответа
	''' </summary>
	''' <param name="contentLength"></param>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Function MakeResponseHeaders(ByVal ContentLength As Long) As String
		' Указание кешу, чтобы различал содержимое методу кодирования (gzip, deflate)
		ResponseHeaders(HttpWorkerRequest.HeaderVary) = "Accept-Encoding"
		' Разрешение выполнять частичный GET
		ResponseHeaders(HttpWorkerRequest.HeaderAcceptRanges) = "bytes"
		' Самореклама
		ResponseHeaders(HttpWorkerRequest.HeaderServer) = "Microsoft-IIS/5.0"
		' Текущая дата
		Dim dNow = DateTime.Now
		ResponseHeaders(HttpWorkerRequest.HeaderDate) = dNow.ToUniversalTime().ToString("R", DateTimeFormatInfo.InvariantInfo)
		' Если не установлен заголовок протухания, то добавить целый месяц
		If ResponseHeaders(HttpWorkerRequest.HeaderExpires) Is Nothing Then
			ResponseHeaders(HttpWorkerRequest.HeaderExpires) = dNow.AddMonths(1).ToString("R", DateTimeFormatInfo.InvariantInfo)
		End If

		' Соединение
		If KeepAlive Then
			If HttpVersion = ReadHeadersResult.HttpVersions.Http10 Then
				' Только при версии протокола 1.0
				ResponseHeaders(HttpWorkerRequest.HeaderConnection) = "Keep-Alive"
			End If
		Else
			ResponseHeaders(HttpWorkerRequest.HeaderConnection) = "Close"
		End If

		' Длина содержимого
		Select Case StatusCode
			Case 100, 204
				' При кодах 100 и 204 тела нет, поэтому его длина не нужна
				ResponseHeaders(HttpWorkerRequest.HeaderContentLength) = Nothing
			Case 304
				If ContentLength > 0 Then
					ResponseHeaders(HttpWorkerRequest.HeaderContentLength) = ContentLength.ToString
				End If
			Case Else
				If ContentLength >= 0 Then
					ResponseHeaders(HttpWorkerRequest.HeaderContentLength) = ContentLength.ToString
				End If
		End Select

		' Сборка всех заголовков в одну строку

		Dim sb As New StringBuilder
		' Код состояния
		sb.AppendLine(String.Join(" ", "HTTP/1.1", StatusCode.ToString, HttpWorkerRequest.GetStatusDescription(StatusCode)))

		For i = 0 To HttpWorkerRequest.ResponseHeaderMaximum - 1
			If ResponseHeaders(i) IsNot Nothing Then
				sb.AppendLine(HttpWorkerRequest.GetKnownResponseHeaderName(i) & ": " & ResponseHeaders(i))
			End If
		Next

		' Дополнительные заголовки
		Dim sHead = sb.Append(UnknownResponseHeaders.ToString).AppendLine.ToString
		' Заголовки запроса и ответа в лог
		ClientConnection.WriteLog(LockObject, Logs, RequestLine & " " & RemoteIP & vbCrLf & AllRawRequestHeaders & sHead)
		Return sHead
	End Function

	''' <summary>
	''' Разбирает запрос клиента
	''' </summary>
	Public Sub ParseRequestLine(ByVal strVirtualPath As String, ByVal strPhysicalDirectory As String)
		' Если есть «?», значит там строка запроса
		Dim iqs As Integer = Url.IndexOf("?"c)
		If iqs > 0 Then
			Path = Url.Substring(0, iqs)
			QueryString = Url.Substring(iqs + 1)
		Else
			Path = Url
		End If

		' Раскодировка пути
		If Path.IndexOf("%"c) >= 0 Then
			Path = HttpUtility.UrlDecode(Path)
		End If

		' Получение имени файла по умолчанию
		If Path.EndsWith("/") Then
			' Сначала проверить на xhtml, htm и html
			' Если не найдено, то использовать "default.aspx"
			Dim flag As Boolean
			For Each DefaultFilename In {"default.xml", "default.xhtml", "default.htm", "default.html"}
				FilePath = Path & DefaultFilename
				If IsBadPath(FilePath) Then
					Exit Sub
				End If
				Dim mappedPath = MapPath(strVirtualPath, FilePath, strPhysicalDirectory)
				If File.Exists(mappedPath) Then
					flag = True
					Exit For
				End If
			Next
			If Not flag Then ' файл по умолчанию не найден
				FilePath = Path & "default.aspx"
			End If
		Else
			FilePath = Path
		End If

		PathTranslated = MapPath(strVirtualPath, FilePath, strPhysicalDirectory)
	End Sub

	Public Function WriteResponseFromBytes(ByVal objStream As Stream, ByVal bytes() As Byte) As Boolean
		If ZipEnabled = ReadHeadersResult.ZipModes.None Then
			Dim HeaderBytes() As Byte = Encoding.ASCII.GetBytes(MakeResponseHeaders(bytes.Length))
			WriteToClient(objStream, HeaderBytes, 0, HeaderBytes.Length)	' заголовки в поток
			Erase HeaderBytes
		End If
		WriteBody(objStream, bytes, 0, bytes.Length)

		Flush(objStream)
		Return True
	End Function

	''' <summary>
	''' Возвращает наличие пути в виртуальном пути
	''' </summary>
	Public Function IsVirtualPathInApp(ByVal strVirtualPath As String, ByVal strPath As String) As Boolean
		If strPath Is Nothing Then
			Return False
		End If
		If strVirtualPath = "/" AndAlso strPath.StartsWith("/") Then
			Return True
		End If

		strPath = CultureInfo.InvariantCulture.TextInfo.ToLower(strPath)

		If strPath.StartsWith(LowerCasedVirtualPathWithTrailingSlash) Then
			Return True
		End If
		If strPath = LowerCasedVirtualPath Then
			Return True
		End If

		Return False
	End Function

	Public Function IsVirtualPathAppPath(ByVal path As String) As Boolean
		If path Is Nothing Then
			Return False
		End If
		path = CultureInfo.InvariantCulture.TextInfo.ToLower(path)
		Return path = LowerCasedVirtualPath OrElse path = LowerCasedVirtualPathWithTrailingSlash
	End Function

	Public Function MapPath(ByVal strVirtualPath As String, ByVal path As String, ByVal PhysicalPath As String) As String
		Dim mappedPath As String = String.Empty

		If String.IsNullOrEmpty(path) OrElse path.Equals("/") OrElse path.Length = 0 Then
			mappedPath = PhysicalPath
		Else
			If IsVirtualPathAppPath(path) Then
				mappedPath = PhysicalPath
			Else
				If IsVirtualPathInApp(strVirtualPath, path) Then
					' Путь внутри приложения
					mappedPath = PhysicalPath & path.Substring(LowerCasedVirtualPathWithTrailingSlash.Length)
				Else
					' Путь извне
					If path.StartsWith("/") Then
						mappedPath = PhysicalPath & path.Substring(1)
					Else
						mappedPath = PhysicalPath & path
					End If
				End If
			End If
		End If
		mappedPath = mappedPath.Replace("/"c, "\"c)
		If mappedPath.EndsWith("\") AndAlso Not mappedPath.EndsWith(":\") Then
			mappedPath = mappedPath.Substring(0, mappedPath.Length - 1)
		End If
		Return mappedPath
	End Function

	Public Function ProcessReadHeadersHost(ByVal objStream As Stream) As Boolean
		Const strVirtualPath As String = "/"
		' Читать запрос клиента
		Select Case ReadAllHeaders(objStream)
			Case ParseRequestLineResult.Success
				' Всё правильно, продолжаем работать дальше
			Case ParseRequestLineResult.HTTPVersionNotSupported
				' Версия не поддерживается
				StatusCode = 505
				WriteError(objStream, HTTPVersionNotSupported, strVirtualPath)
				Return True
			Case ParseRequestLineResult.MethodNotSupported
				' Метод не поддерживается сервером
				StatusCode = 501
				ResponseHeaders(HttpWorkerRequest.HeaderAllow) = ClientConnection.AllSupportHttpMethods
				WriteError(objStream, String.Format(ClientConnection.MethodNotAllowed, HttpMethod), strVirtualPath)
				Return True
			Case ParseRequestLineResult.HostRequired
				' Требуется хост
				StatusCode = 403
				WriteError(objStream, ClientConnection.ErrorHost, strVirtualPath)
				Return True
			Case ParseRequestLineResult.BadRequest
				' Плохой запрос
				StatusCode = 400
				WriteError(objStream, ClientConnection.ErrorBadRequest, strVirtualPath)
				Return True
			Case ParseRequestLineResult.EmptyRequest
				' Пустой запрос, клиент закрыл соединение
				Return True
		End Select

		' Проверить заголовок Host, так как он обязателен
		If WebSites.ContainsKey(RequestHeaders(HttpWorkerRequest.HeaderHost)) Then
			Return False
		Else
			' Host был неверный
			StatusCode = 403 ' какой код отправлять при неверном Host?
			WriteError(objStream, ClientConnection.ErrorHost, strVirtualPath)
			Return True
		End If
	End Function

End Structure
