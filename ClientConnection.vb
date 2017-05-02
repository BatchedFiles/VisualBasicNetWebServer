Public Module ClientConnection

#Region "Константы"

	''' <summary>
	''' Все поддерживаемые методы HTTP
	''' </summary>
	Public Const AllSupportHttpMethods = "COPY, DELETE, GET, HEAD, MOVE, OPTIONS, PATCH, POST, PROPFIND, PUT, TRACE"
	Public Const AllSupportHttpMethodsWithoutPut = "COPY, DELETE, GET, HEAD, MOVE, OPTIONS, POST, PROPFIND, TRACE"
	''' <summary>
	''' Все поддерживаемые методы самим сервером
	''' </summary>
	Public Const AllSupportHttpMethodsServer = "GET, HEAD, OPTIONS, TRACE"
	''' <summary>
	''' Все поддерживаемые методы интерпретатором
	''' </summary>
	Public AllSupportHttpMethodsProcessor() As ReadHeadersResult.HttpMethods = {ReadHeadersResult.HttpMethods.HttpCopy, ReadHeadersResult.HttpMethods.HttpDelete, ReadHeadersResult.HttpMethods.HttpMove, ReadHeadersResult.HttpMethods.HttpPatch, ReadHeadersResult.HttpMethods.HttpPost, ReadHeadersResult.HttpMethods.HttpPropfind}

	''' <summary>
	''' Плохой запрос
	''' </summary>
	Public Const ErrorBadRequest = "Плохой запрос"
	''' <summary>
	''' Неправильный заголовок Host
	''' </summary>
	Public Const ErrorHost = "У тебя нет привилегий доступа к этому сайту, простолюдин. Этот сайт предназначен только для благородных господ."
	''' <summary>
	''' Файл не может быть исполнен процессором
	''' </summary>
	Public Const Error403 = "У тебя нет привилегий доступа к этому файлу, простолюдин. Файлы такого типа предназначены только для благородных господ."
	''' <summary>
	''' В запросе присутствуют запрещённые символы
	''' </summary>
	Public Const ErrorBadPath = "В запросе присутствуют запрещённые символы"
	''' <summary>
	''' Протокол не поддерживается
	''' </summary>
	Public Const HTTPVersionNotSupported = "Версия протокола не поддерживается"
	''' <summary>
	''' Метод не поддерживается
	''' </summary>
	Public Const MethodNotAllowed = "Метод {0} не поддерживается"
	''' <summary>
	''' Клиентские скрипты не найдены
	''' </summary>
	Public Const ClientScriptNotFound = "Клиентские скрипты не найдены"
	Public Const FileNotAvailable = "Не могу прочитать этот файл, возможно он занят другим процессом"
	Public Const FileNotFound = "Файл {0} не найден. Возможно он будет найден когда‐нибудь потом."
	Public Const FileGone = "Файл {0} удалён навсегда. Он никогда не будет найден. Необходимо удалить все ссылки на него."
	Public Const MethodNotAllowedFile = "К файлам типа {0} нельзя применять метод {1}"
	Public Const RequestEntityTooLarge = "Длина тела запроса слишком большая, разрешается не больше 4194304 байт"
	Public Const LengthRequired = "Для метода {0} требуется указание длины тела запроса"
	Public Const MovedPermanently = "Ресурс перекатился на адрес <a href=""{0}"">{0}</a>. Тебе нужно идти туда."
#End Region

	Public Enum ParseRequestLineResult
		''' <summary>
		''' Ошибок нет
		''' </summary>
		Success
		''' <summary>
		''' Версия протокола не поддерживается
		''' </summary>
		HTTPVersionNotSupported
		''' <summary>
		''' Метод не поддерживается сервером
		''' </summary>
		''' <remarks></remarks>
		MethodNotSupported
		''' <summary>
		''' Нужен заголовок Host
		''' </summary>
		HostRequired
		''' <summary>
		''' Ошибка в запросе, синтаксисе запроса
		''' </summary>
		BadRequest
		''' <summary>
		''' Клиент закрыл соединение
		''' </summary>
		EmptyRequest
	End Enum

	''' <summary>
	''' Результат чтения отправленного клиентом тела
	''' </summary>
	Public Enum ParsePostedContentResult
		''' <summary>
		''' Ошибок нет
		''' </summary>
		None
		''' <summary>
		''' Тело слишком длинное
		''' </summary>
		RequestEntityTooLarge
		''' <summary>
		''' Требуется указание длины
		''' </summary>
		LengthRequired
		''' <summary>
		'''  Плохой запрос (ошибка в получении длины тела)
		''' </summary>
		BadRequest
	End Enum

	Public Structure TextFileBytes
		Public FileBytes() As Byte
		Public ZipMode As ReadHeadersResult.ZipModes
	End Structure

	Private WithEvents tmrLog As Timers.Timer

	Private m_Any As Boolean
	Private m_ServerPort As Integer
	Private m_Listener As TcpListener

	''' <summary>
	''' Миме
	''' </summary>
	Private m_ContentTypes As List(Of ContentType)

	''' <summary>
	''' Расширения исполняемых файлов
	''' </summary>
	Private m_AspNetProcessingFiles As List(Of String)

	''' <summary>
	''' Список сайтов
	''' </summary>
	Private m_WebSites As Dictionary(Of String, WebSite)

	''' <summary>
	''' Список хостов
	''' </summary>
	Private m_Hosts As Dictionary(Of String, Host)

	''' <summary>
	''' Список логов сервера
	''' </summary>
	Private m_Logs As MemoryStream

	''' <summary>
	''' Синхронизатор при ведении логов
	''' </summary>
	Private m_Lock As Object

	''' <summary>
	''' Путь к исполняемому файлу
	''' </summary>
	Private m_ExePath As String

	Private m_localEP As String

	''' <summary>
	''' Записывает байты в поток клиента
	''' </summary>
	''' <param name="bytes"></param>
	''' <param name="offset"></param>
	''' <param name="length"></param>
	''' <remarks></remarks>
	Public Sub WriteToClient(ByVal objStream As Stream, ByVal bytes() As Byte, ByVal offset As Integer, ByVal length As Integer)
		Try
			objStream.Write(bytes, offset, length)
		Catch ex As Exception
		End Try
	End Sub

	''' <summary>
	''' Проверяет путь на запрещённые символы
	''' </summary>
	''' <param name="strPath">Путь для проверки</param>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Function IsBadPath(ByVal strPath As String) As Boolean
		If strPath Is Nothing Then
			Return True
		End If
		If strPath.IndexOfAny({"%"c, ">"c, "<"c, "$"c, ":"c}) >= 0 Then
			Return True
		End If
		If strPath.Any(Function(c As Char) Convert.ToInt32(c) < 32) Then
			Return True
		End If
		If strPath.IndexOfAny(Path.GetInvalidPathChars) >= 0 Then
			Return True
		End If
		If strPath.IndexOf("..") >= 0 Then
			Return True
		End If
		Return False
	End Function

	''' <summary>
	''' Возвращает байты текстовой файлы
	''' </summary>
	''' <param name="fileName"></param>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Function GetFileBytes(ByVal fileName As String) As TextFileBytes
		'Dim bytesRead As Integer
		Dim fileBytes() As Byte
		Try
			Using fs As New FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)

				'If IsText Then
				Using sr As New StreamReader(fs)
					Dim strFile As String = sr.ReadToEnd
					sr.Close()
					fileBytes = Encoding.UTF8.GetBytes(strFile)
				End Using
				'bytesRead = fileBytes.Length
				'Else
				'	Dim len As Integer = Convert.ToInt32(fs.Length)
				'	ReDim fileBytes(len - 1)
				'	bytesRead = fs.Read(fileBytes, 0, len)
				'	ReDim Preserve fileBytes(bytesRead - 1)
				'End If
				fs.Close()
			End Using
		Catch ex As Exception
			Return Nothing
		End Try
		GetFileBytes.FileBytes = fileBytes
	End Function

	''' <summary>
	''' Чтение конфигурации
	''' </summary>
	''' <remarks></remarks>
	Private Sub ReadConfiguration()

		m_Any = Convert.ToBoolean(ConfigurationManager.AppSettings("AnyAdapter"))
		m_ServerPort = Integer.Parse(ConfigurationManager.AppSettings("ServerPort"))

		Dim config As System.Configuration.Configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
		Dim myGroup As WebServerSectionGroup = CType(config.GetSectionGroup("webServer"), WebServerSectionGroup)

		' Миме
		For Each ct As ContentTypeElement In myGroup.ContentTypes.ContentTypes
			m_ContentTypes.Add(New ContentType With {.Extension = ct.Extension, .ContentType = ct.ContentType, .IsTextFormat = ct.IsTextFormat})
		Next
		' Исполняемые файлы
		For Each fa As AspNetProcessingFilesElement In myGroup.AspNetProcessingFiles.AspNetProcessingFiles
			m_AspNetProcessingFiles.Add(fa.Extension)
		Next
		' Сайты
		For Each ws As WebSitesElement In myGroup.WebSites.WebSites
			m_WebSites.Add(ws.HostName, New WebSite With {.ServerCertificate = If(ws.EnableSsl, X509Certificate.CreateFromCertFile(ws.CertificateFilePath), Nothing), .HostName = ws.HostName, .PhysicalDirectory = ws.PhysicalDirectory, .VirtualPath = ws.VirtualPath, .IsMoved = ws.IsMoved, .MovedUrl = ws.MovedUrl})
		Next

	End Sub

	''' <summary>
	''' Создаёт исполняемую среду Asp.Net без регистрации в глобальном хранилище сборок
	''' </summary>
	''' <param name="virtualPath"></param>
	''' <param name="physicalPath"></param>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Function CreateWorkerAppDomainWithHost(ByVal virtualPath As String, ByVal physicalPath As String) As Host
		Dim hostType As System.Type = GetType(Host)
		Dim uniqueAppString As String = String.Concat(virtualPath, physicalPath).ToLowerInvariant()
		Dim appId As String = (uniqueAppString.GetHashCode()).ToString("x", CultureInfo.InvariantCulture)
		Dim appManager As ApplicationManager = ApplicationManager.GetApplicationManager()

		Dim buildManagerHostType As System.Type = GetType(HttpRuntime).Assembly.[GetType]("System.Web.Compilation.BuildManagerHost")
		Dim buildManagerHost As IRegisteredObject = appManager.CreateObject(appId, buildManagerHostType, virtualPath, physicalPath, False)

		buildManagerHostType.InvokeMember("RegisterAssembly", BindingFlags.Instance Or BindingFlags.InvokeMethod Or BindingFlags.NonPublic, Nothing, buildManagerHost, New Object() {hostType.Assembly.FullName, hostType.Assembly.Location})
		Return DirectCast(appManager.CreateObject(appId, hostType, virtualPath, physicalPath, False), Host)
	End Function

	Private Sub OnSocketAccept(ByVal o As Object)
		Dim objState As StateObject = CType(o, StateObject)
		Dim objClient As TcpClient = objState.Client
		' Ожидать чтения данных с клиента 5 минут
		objClient.ReceiveTimeout = 300 * 1000
		Debug.WriteLine("Поток {0} стартовал", objState.Number)

		Dim objStream As Stream = Nothing
		Dim endPoint As IPEndPoint = Nothing
		Try
			endPoint = CType(objClient.Client.RemoteEndPoint, IPEndPoint)
			objStream = objClient.GetStream
		Catch ex As Exception
			' Закрыть соединение
			WriteLog(m_Lock, m_Logs, "Произошло исключение при попытке получения objStream " & ex.ToString)
			If objStream IsNot Nothing Then
				objStream.Close()
			End If
			If objClient IsNot Nothing Then
				objClient.Close()
			End If
			Exit Sub
		End Try

		' Подготовить объект состояния для запроса клиента
		Dim state As ReadHeadersResult
		With state
			.ContentTypes = m_ContentTypes
			.LocalIp = m_localEP
			.RemoteIP = If(endPoint IsNot Nothing AndAlso endPoint.Address IsNot Nothing, endPoint.Address.ToString, "127.0.0.1")
			.Logs = m_Logs
			.LockObject = m_Lock
			.WebSites = m_WebSites
			.IsSecure = False
		End With

		Do
			' Инициализация объекта состояния в начальное значение
			With state
				.StatusCode = 200
				.UnknownResponseHeaders = New StringBuilder
				ReDim .HeaderBytes(ReadHeadersResult.MaxRequestHeaderBytes - 1)
				ReDim .UnknownRequestHeadersName(ReadHeadersResult.UnknownRequestHeadersMaximum - 1)
				ReDim .UnknownRequestHeadersValue(ReadHeadersResult.UnknownRequestHeadersMaximum - 1)
				ReDim .RequestHeaders(HttpWorkerRequest.RequestHeaderMaximum - 1)
				ReDim .ResponseHeaders(HttpWorkerRequest.ResponseHeaderMaximum - 1)

				.ZipEnabled = ReadHeadersResult.ZipModes.None
				.HeadersSent = False
				.UnknownRequestHeadersCount = 0
				.RequestBodyContentLength = 0
				.KeepAlive = False
				.SendOnlyHeaders = False
				.HttpMethod = ReadHeadersResult.HttpMethods.None
				.HttpVersion = ReadHeadersResult.HttpVersions.Http11
				.EndHeadersOffset = 0
				.StartHeadersOffset = 0
				.HeaderBytesLength = 0

				.RequestLine = Nothing
				.Url = Nothing
				.Path = Nothing
				.FilePath = Nothing
				.PathTranslated = Nothing
				.QueryString = Nothing
				.AllRawRequestHeaders = Nothing
				.ServerName = Nothing
				.MemoryStream = Nothing
				.ZipStream = Nothing
				.LowerCasedVirtualPath = Nothing
				.LowerCasedVirtualPathWithTrailingSlash = Nothing
			End With

			' Получить заголовки запроса и проверить Host
			If state.ProcessReadHeadersHost(objStream) Then
				' Какая‐то ошибка, выходим
				Exit Do
			End If

			' Найти сайт по его имени
			Dim www = m_WebSites.Item(state.RequestHeaders(HttpWorkerRequest.HeaderHost))
			If www.IsMoved AndAlso state.Url <> "/robots.txt" Then
				' Сайт перемещён на другой ресурс
				' если запрошен документ /robots.txt то не перенаправлять
				' нужно перенаправить на него ответом 301 Found
				state.ResponseHeaders(HttpWorkerRequest.HeaderLocation) = www.MovedUrl & state.Url
				state.StatusCode = 301
				state.WriteError(objStream, String.Format(MovedPermanently, www.MovedUrl), www.VirtualPath)
				Exit Do
			End If

			' Теперь знаем виртуальный путь приложения
			state.ServerName = www.HostName
			' Пути
			state.LowerCasedVirtualPath = CultureInfo.InvariantCulture.TextInfo.ToLower(www.VirtualPath)
			state.LowerCasedVirtualPathWithTrailingSlash = CultureInfo.InvariantCulture.TextInfo.ToLower(If(www.VirtualPath.EndsWith("/"), www.VirtualPath, www.VirtualPath & "/"))

			' Разобрать запрос и получить имя файла по умолчанию
			state.ParseRequestLine(www.VirtualPath, www.PhysicalDirectory)

			' Проверить запрос на плохие символы
			If ClientConnection.IsBadPath(state.FilePath) Then
				' Плохой запрос
				state.StatusCode = 400
				state.WriteError(objStream, ErrorBadPath, www.VirtualPath)
				Exit Do
			End If

			' Обработка запроса

			Dim fileExtention As String = Path.GetExtension(state.PathTranslated)

			Select Case state.HttpMethod
				Case ReadHeadersResult.HttpMethods.HttpGet, ReadHeadersResult.HttpMethods.HttpHead
					' Файла, обрабатываемая сервером без asp.net
					Dim kvp = m_ContentTypes.Find(Function(x) x.Extension = fileExtention)
					If kvp IsNot Nothing Then

						If File.Exists(state.PathTranslated) Then
							' Прочитать дополнительные заголовки ответа для файлы
							Dim sExtHeadersFile As String = state.PathTranslated & ".headers"
							If File.Exists(sExtHeadersFile) Then
								For Each Line In File.ReadAllLines(sExtHeadersFile)
									Dim c As Integer = Line.IndexOf(":"c)
									If c > 0 Then
										Dim name As String = Line.Substring(0, c)
										Dim value As String = Line.Substring(c + 1)
										state.AddResponseHeader(name, value)
									End If
								Next
							End If

							' Нужно проверить частичный запрос
							If state.RequestHeaders(HttpWorkerRequest.HeaderRange) Is Nothing Then
								' Выдать всё содержимое от начала до конца
							Else
								' Выдать только диапазон
								'Content-Range: bytes 471104-2355520/2355521
								'state.RequestHeaders(HttpWorkerRequest.HeaderContentRange) = "bytes 471104-2355520/2355521"
							End If

							If kvp.IsTextFormat Then
								' Это текстовая файла, нужно получить UTF-8 байты без метки BOM
								' затем каким‐то образом получить сжатое содержимое
								' и отправить его
								Dim fb = ClientConnection.GetFileBytes(state.PathTranslated)
								If fb.FileBytes Is Nothing Then
									' Чтение файлы завершилось неудачей
									state.StatusCode = 500
									state.WriteError(objStream, FileNotAvailable, www.VirtualPath)
									Exit Do
								Else
									If fb.FileBytes.Length > 512 Then
										state.AddCompressionMethodHeader(kvp.IsTextFormat)
									End If
									state.AddCacheHeaders()
									state.ResponseHeaders(HttpWorkerRequest.HeaderContentType) = kvp.ContentType & "; charset=utf-8"

									state.WriteResponseFromBytes(objStream, fb.FileBytes)
								End If
							Else
								state.AddCacheHeaders()
								state.ResponseHeaders(HttpWorkerRequest.HeaderContentType) = kvp.ContentType
								' Это двоичный файл, получить файловый поток
								' и потом скопировать его в сетевой поток
								Using fs As New FileStream(state.PathTranslated, FileMode.Open, FileAccess.Read)
									state.WriteHeaders(objStream, fs.Length)
									' Не отправлять тело, если нужны только заголовки
									If Not state.SendOnlyHeaders Then
										Try
											fs.CopyTo(objStream)
										Catch ex As Exception
											' Чтение файла завершилось неудачей
											state.StatusCode = 500
											state.WriteError(objStream, FileNotAvailable, www.VirtualPath)
											Exit Do
										End Try
									End If
								End Using
								state.Flush(objStream)
							End If
						Else
							' Файла не существет
							If File.Exists(state.PathTranslated & ".410") Then
								' Файла раньше существовала, но теперь удалена навсегда
								state.StatusCode = 410
								state.WriteError(objStream, String.Format(FileGone, state.FilePath), www.VirtualPath)
							Else
								' Файлы не существует, но она может появиться позже
								state.StatusCode = 404
								state.WriteError(objStream, String.Format(FileNotFound, state.FilePath), www.VirtualPath)
							End If
							Exit Do
						End If
						GoTo ReadAllHeadersAgain
					End If
				Case ReadHeadersResult.HttpMethods.HttpPut
					' Проверить существование фала с логинами и паролями
					' Если не существует, то метод на сервере не реализован
					' Где взять логины и пароли
					' создать специальный файл для этого
					' например, в корне сайта лежит файл users, без расширения
					' В этом файле указать
					' логины и пароли
					' типы принимаемых файлов
					' максимальную длину принимаемого содержимого
					Dim UsersFile As String = state.MapPath(www.VirtualPath, "/users", www.PhysicalDirectory)
					If File.Exists(UsersFile) Then
						If state.HttpAuth(objStream, UsersFile, www.VirtualPath) Then
							' Клиент не прошёл аутентификацию, выходим
							Exit Do
						End If
						' Если какой-то из переданных серверу заголовков Content-* не опознан или не может быть использован в данной ситуации
						' сервер возвращает статус ошибки 501 (Not Implemented).

						'Content-Length: 2454
						' Получить предварительно загруженные байты
						' Если требуется — получить дополнительные байты
						' Проверка на длину отправленного содержимого
						Select Case state.ParsePostedContent()
							Case ParsePostedContentResult.RequestEntityTooLarge
								' 413 Request Entity Too Large — 
								' сервер отказывается обработать запрос по причине
								' слишком большого размера тела запроса
								' Сервер может закрыть соединение,
								' чтобы прекратить дальнейшую передачу запроса.
								' Если проблема временная, то рекомендуется в ответ сервера
								' включить заголовок Retry-After с указанием времени,
								' по истечении которого можно повторить аналогичный запрос.
								' Появился в HTTP/1.1.
								state.StatusCode = 413
								state.WriteError(objStream, RequestEntityTooLarge, www.VirtualPath)
								Exit Do
							Case ParsePostedContentResult.LengthRequired
								' 411 Length Required — для указанного ресурса
								'клиент должен указать Content-Length в заголовке запроса.
								' Без указания этого поля не стоит делать повторную попытку запроса к серверу по данному URI.
								' Такой ответ естественен для запросов типа POST и PUT.
								' Например, если по указанному URI производится загрузка файлов,
								' а на сервере стоит ограничение на их объём.
								' Тогда разумней будет проверить в самом начале заголовок Content-Length
								' и сразу отказать в загрузке, чем провоцировать бессмысленную нагрузку,
								' Появился в HTTP/1.1.
								state.StatusCode = 411
								state.WriteError(objStream, String.Format(LengthRequired, state.HttpMethod), www.VirtualPath)
								Exit Do
							Case ParsePostedContentResult.BadRequest
								' Ошибка в синтаксисе запроса
								state.StatusCode = 400
								state.WriteError(objStream, ErrorBadRequest, www.VirtualPath)
								Exit Do
						End Select

						'Content-Type: text/html; encoding=utf-8
						' Принимать только неинтерпретируемое содержимое
						If String.IsNullOrEmpty(state.RequestHeaders(HttpWorkerRequest.HeaderContentType)) Then
							' Не указано содержимое
							state.StatusCode = 501
							state.ResponseHeaders(HttpWorkerRequest.HeaderAllow) = AllSupportHttpMethodsWithoutPut
							state.WriteError(objStream, String.Format(MethodNotAllowed, state.HttpMethod), www.VirtualPath)
							Exit Do
						End If
						' Может быть указана кодировка содержимого
						Dim contentType() As String = state.RequestHeaders(HttpWorkerRequest.HeaderContentType).Split(";"c)
						Dim kvp = m_ContentTypes.Find(Function(x) x.ContentType = contentType(0))
						If kvp Is Nothing Then
							' Такое содержимое нельзя загружать
							state.StatusCode = 501
							state.ResponseHeaders(HttpWorkerRequest.HeaderAllow) = AllSupportHttpMethodsWithoutPut
							state.WriteError(objStream, String.Format(MethodNotAllowed, state.HttpMethod), www.VirtualPath)
							Exit Do
						End If
						' изменить расширение файла на правильное
						' нельзя оставлять отправленное пользователем расширение
						' указать (новое) имя файла в заголовке Location
						state.FilePath = Path.ChangeExtension(state.FilePath, kvp.Extension)
						state.PathTranslated = state.MapPath(www.VirtualPath, state.FilePath, www.PhysicalDirectory)

						'Content-Language: ru, ru-RU
						' язык можно записать в специальный файл
						If Not String.IsNullOrEmpty(state.RequestHeaders(HttpWorkerRequest.HeaderContentLanguage)) Then
							' Если файл уже существует, всё равно перезаписать
							Using fs As New FileStream(state.PathTranslated & ".headers", FileMode.Create, FileAccess.Write)
								Using sw As New StreamWriter(fs)
									sw.WriteLine("Content-Language:" & state.RequestHeaders(HttpWorkerRequest.HeaderContentLanguage))
									sw.Close()
								End Using
								fs.Close()
							End Using
							state.ResponseHeaders(HttpWorkerRequest.HeaderContentLanguage) = state.RequestHeaders(HttpWorkerRequest.HeaderContentLanguage)
						End If

						'Content-Encoding: gzip, deflate
						' Если указано сжатие, то создать сжатый поток и распаковать

						If Not String.IsNullOrEmpty(state.RequestHeaders(HttpWorkerRequest.HeaderContentEncoding)) Then
							If state.RequestHeaders(HttpWorkerRequest.HeaderContentEncoding) = "gzip" Then
								' Создать поток gzip
							Else
								If state.RequestHeaders(HttpWorkerRequest.HeaderContentEncoding) = "deflate" Then
									' Создать поток deflate
								End If
							End If
						End If

						' Прочитать байты
						' Если есть предварительно загруженные данные
						' То скопировать их
						Dim bytes(state.RequestBodyContentLength - 1) As Byte
						' Предварительно загруженные байты
						' Длина всего тела запроса
						Dim PreloadedContentLength As Integer = state.HeaderBytesLength - state.EndHeadersOffset
						If PreloadedContentLength > 0 Then
							Buffer.BlockCopy(state.HeaderBytes, state.EndHeadersOffset, bytes, 0, PreloadedContentLength)
						End If

						' затем получить всё остальное
						Do While PreloadedContentLength < state.RequestBodyContentLength
							Dim numReceived As Integer
							Try
								numReceived = objStream.Read(bytes, PreloadedContentLength, state.RequestBodyContentLength - PreloadedContentLength)
							Catch ex As Exception
							End Try
							If numReceived = 0 Then
								Exit Do
							Else
								' Сколько байт получили, на столько и увеличили буфер
								PreloadedContentLength += numReceived
							End If
						Loop

						' В случае отсутствия ресурса по указанному в заголовке URI,
						' сервер создает его и возвращает код статуса 201 (Created),
						' если ресурс присутствовал и был изменен в результате запроса PUT,
						' выдается код статуса 200 (Ok) или 204 (No Content). 
						If File.Exists(state.PathTranslated) Then
							state.StatusCode = 200
						Else
							state.StatusCode = 201
							state.ResponseHeaders(HttpWorkerRequest.HeaderLocation) = "http://" & www.HostName & HttpUtility.HtmlEncode(state.FilePath)
						End If

						' Теперь записать данные на диск
						Using fs As New FileStream(state.PathTranslated, FileMode.Create, FileAccess.Write)
							' Определить отправленную кодировку
							'If kvp.IsTextFormat Then
							'Else
							fs.Write(bytes, 0, PreloadedContentLength)
							'End If
							fs.Close()
						End Using
						' Отправить заголовки
						' Отправить тело, если есть
						If kvp.IsTextFormat Then
							' Если это текстовый файл, нужно получить UTF-8 байты без метки BOM
							' и указать кодировку "; charset=utf-8"
							Dim fileBytes = ClientConnection.GetFileBytes(state.PathTranslated)
							If fileBytes.FileBytes.Length > 512 Then
								state.AddCompressionMethodHeader(kvp.IsTextFormat)
							End If
							state.ResponseHeaders(HttpWorkerRequest.HeaderContentType) = kvp.ContentType & "; charset=utf-8"
							state.WriteResponseFromBytes(objStream, fileBytes.FileBytes)
						Else
							state.ResponseHeaders(HttpWorkerRequest.HeaderContentType) = kvp.ContentType
							' Это двоичный файл, получить файловый поток
							' и потом скопировать его в сетевой поток
							Using fs As New FileStream(state.PathTranslated, FileMode.Open, FileAccess.Read)
								state.WriteHeaders(objStream, fs.Length)
								' Не отправлять тело, если нужны только заголовки
								Try
									fs.CopyTo(objStream)
								Catch ex As Exception
									' Чтение файла завершилось неудачей
									state.StatusCode = 500
									state.WriteError(objStream, FileNotAvailable, www.VirtualPath)
									Exit Do
								End Try
							End Using
							state.Flush(objStream)
						End If

						' Удалить файл 410, если он был
						File.Delete(state.PathTranslated & ".410")

						' Если ресурс с указанным URI не может быть создан или модифицирован,
						' должно быть послано соответствующее сообщение об ошибке. 

						'' Всё хорошо
						GoTo ReadAllHeadersAgain
					Else
						' Метод не поддерживается сервером
						state.StatusCode = 501
						state.ResponseHeaders(HttpWorkerRequest.HeaderAllow) = AllSupportHttpMethodsWithoutPut
						state.WriteError(objStream, String.Format(MethodNotAllowed, state.HttpMethod), www.VirtualPath)
						Exit Do
					End If
				Case ReadHeadersResult.HttpMethods.HttpPatch
					' Если файл обрабатывается процессором
					If Not m_AspNetProcessingFiles.Contains(fileExtention) Then
						' Обрабатывается только процессором
						state.StatusCode = 405
						state.ResponseHeaders(HttpWorkerRequest.HeaderAllow) = AllSupportHttpMethodsServer
						state.WriteError(objStream, String.Format(MethodNotAllowedFile, fileExtention, state.HttpMethod), www.VirtualPath)
						Exit Do
					End If
				Case ReadHeadersResult.HttpMethods.HttpDelete
					If m_AspNetProcessingFiles.Contains(fileExtention) Then
						' Обрабатывается процессором
					Else
						Dim UsersFile As String = state.MapPath(www.VirtualPath, "/users", www.PhysicalDirectory)
						If File.Exists(UsersFile) Then
							If File.Exists(state.PathTranslated) Then
								' Проверка заголовка Authorization
								If state.HttpAuth(objStream, UsersFile, www.VirtualPath) Then
									Exit Do
								End If

								' Необходимо удалить файл
								File.Delete(state.PathTranslated)

								' затем удалить возможные заголовочные файлы
								Dim sHeaderFile = state.PathTranslated & ".headers"
								If File.Exists(sHeaderFile) Then
									File.Delete(sHeaderFile)
								End If

								' и создать файл, показывающий, что файл был удалён
								Using fs As New FileStream(state.PathTranslated & ".410", FileMode.Create)
									fs.Close()
								End Using

								' Отправить заголовки, что нет содержимого
								state.StatusCode = 204
								state.WriteHeaders(objStream, 0)
								state.Flush(objStream)

								GoTo ReadAllHeadersAgain
							Else
								' Файла не существет
								If File.Exists(state.PathTranslated & ".410") Then
									' Файла существовала, но теперь удалён навсегда
									state.StatusCode = 410
									state.WriteError(objStream, String.Format(FileGone, state.FilePath), www.VirtualPath)
								Else
									' Файлы не существует, но может появиться позже
									state.StatusCode = 404
									state.WriteError(objStream, String.Format(FileNotFound, state.FilePath), www.VirtualPath)
								End If
								Exit Do
							End If
						Else
							' Метод не поддерживается сервером
							state.StatusCode = 501
							state.ResponseHeaders(HttpWorkerRequest.HeaderAllow) = AllSupportHttpMethodsWithoutPut
							state.WriteError(objStream, String.Format(MethodNotAllowed, state.HttpMethod), www.VirtualPath)
							Exit Do
						End If
					End If
				Case ReadHeadersResult.HttpMethods.HttpPost
					If Not m_AspNetProcessingFiles.Contains(fileExtention) Then
						' Обрабатывается только процессором
						state.StatusCode = 405
						state.ResponseHeaders(HttpWorkerRequest.HeaderAllow) = AllSupportHttpMethodsServer
						state.WriteError(objStream, String.Format(MethodNotAllowedFile, fileExtention, state.HttpMethod), www.VirtualPath)
						Exit Do
					End If
				Case ReadHeadersResult.HttpMethods.HttpOptions
					state.StatusCode = 204 ' нет содержимого
					' Если звёздочка, то ко всему серверу
					If state.Path = "*" Then
						state.ResponseHeaders(HttpWorkerRequest.HeaderAllow) = AllSupportHttpMethods
					Else
						' К конкретному ресурсу
						If m_AspNetProcessingFiles.Contains(fileExtention) Then
							' Файл обрабатывается процессором, значит может обработать разные методы
							state.ResponseHeaders(HttpWorkerRequest.HeaderAllow) = AllSupportHttpMethods
						Else
							state.ResponseHeaders(HttpWorkerRequest.HeaderAllow) = AllSupportHttpMethodsServer
						End If
					End If
					Dim HeaderBytes() As Byte = Encoding.ASCII.GetBytes(state.MakeResponseHeaders(0))
					ClientConnection.WriteToClient(objStream, HeaderBytes, 0, HeaderBytes.Length)
					GoTo ReadAllHeadersAgain
				Case ReadHeadersResult.HttpMethods.HttpTrace
					' Собрать все заголовки запроса и сформировать из них тело ответа
					With state
						.StatusCode = 200
						.ResponseHeaders(HttpWorkerRequest.HeaderContentType) = "message/http; charset=8bit"
						.ResponseHeaders(HttpWorkerRequest.HeaderContentLanguage) = "ru-RU,ru"
					End With
					Dim BodyBytes = Encoding.ASCII.GetBytes(state.RequestLine & vbCrLf & state.AllRawRequestHeaders)
					If BodyBytes.Length > 512 Then
						state.AddCompressionMethodHeader(True)
					End If
					state.WriteResponseFromBytes(objStream, BodyBytes)
					GoTo ReadAllHeadersAgain
				Case ReadHeadersResult.HttpMethods.HttpCopy
					If Not m_AspNetProcessingFiles.Contains(fileExtention) Then
						' Обрабатывается только процессором
						state.StatusCode = 405
						state.ResponseHeaders(HttpWorkerRequest.HeaderAllow) = AllSupportHttpMethodsServer
						state.WriteError(objStream, String.Format(MethodNotAllowedFile, fileExtention, state.HttpMethod), www.VirtualPath)
						Exit Do
					End If
				Case ReadHeadersResult.HttpMethods.HttpMove
					If Not m_AspNetProcessingFiles.Contains(fileExtention) Then
						' Обрабатывается только процессором
						state.StatusCode = 405
						state.ResponseHeaders(HttpWorkerRequest.HeaderAllow) = AllSupportHttpMethodsServer
						state.WriteError(objStream, String.Format(MethodNotAllowedFile, fileExtention, state.HttpMethod), www.VirtualPath)
						Exit Do
					End If
				Case ReadHeadersResult.HttpMethods.HttpPropfind
					' Если файл обрабатывается процессором
					If Not m_AspNetProcessingFiles.Contains(fileExtention) Then
						' Обрабатывается только процессором
						state.StatusCode = 405
						state.ResponseHeaders(HttpWorkerRequest.HeaderAllow) = AllSupportHttpMethodsServer
						state.WriteError(objStream, String.Format(MethodNotAllowedFile, fileExtention, state.HttpMethod), www.VirtualPath)
						Exit Do
					End If
			End Select

			' Управление дошло до этого места, значит
			' Файл обрабатывается Asp.Net процессором

			' Проверка на допустимый тип файла
			If m_AspNetProcessingFiles.Contains(fileExtention) Then

				If AllSupportHttpMethodsProcessor.Contains(state.HttpMethod) Then
					' Проверка на длину отправленного содержимого
					Select Case state.ParsePostedContent()
						Case ParsePostedContentResult.RequestEntityTooLarge
							' 413 Request Entity Too Large — 
							' сервер отказывается обработать запрос по причине
							' слишком большого размера тела запроса
							' Сервер может закрыть соединение,
							' чтобы прекратить дальнейшую передачу запроса.
							' Если проблема временная, то рекомендуется в ответ сервера
							' включить заголовок Retry-After с указанием времени,
							' по истечении которого можно повторить аналогичный запрос.
							' Появился в HTTP/1.1.
							state.StatusCode = 413
							state.WriteError(objStream, RequestEntityTooLarge, www.VirtualPath)
							Exit Do
						Case ParsePostedContentResult.LengthRequired
							' 411 Length Required — для указанного ресурса
							'клиент должен указать Content-Length в заголовке запроса.
							' Без указания этого поля не стоит делать повторную попытку запроса к серверу по данному URI.
							' Такой ответ естественен для запросов типа POST и PUT.
							' Например, если по указанному URI производится загрузка файлов,
							' а на сервере стоит ограничение на их объём.
							' Тогда разумней будет проверить в самом начале заголовок Content-Length
							' и сразу отказать в загрузке, чем провоцировать бессмысленную нагрузку,
							' разрывая соединение, когда клиент действительно пришлёт слишком объёмное сообщение.
							' Появился в HTTP/1.1.
							state.StatusCode = 411
							state.WriteError(objStream, String.Format(LengthRequired, state.HttpMethod), www.VirtualPath)
							Exit Do
					End Select
				End If

				If state.HttpVersion = ReadHeadersResult.HttpVersions.Http11 Then
					If Not String.IsNullOrEmpty(state.ResponseHeaders(HttpWorkerRequest.HeaderExpect)) Then
						If state.ResponseHeaders(HttpWorkerRequest.HeaderExpect).Contains("100-continue") Then
							' Если есть данные от клиента, то отправить ответ 100
							' ответ с кодом 100 может быть только в версии HTTP/1.1
							' Если есть заголовок Expect: 100-continue
							If state.RequestBodyContentLength > 0 AndAlso state.HeaderBytesLength - state.EndHeadersOffset < state.RequestBodyContentLength Then
								state.StatusCode = 100
								state.Write100Continue(objStream)
							End If
						End If
					End If
				End If

				If state.HttpMethod = ReadHeadersResult.HttpMethods.HttpHead Then
					' Грязный хак, чтобы заставить процессор отдавать тело ответа
					' для правильного вычисления длины тела ответа при включении сжатия
					state.HttpMethod = ReadHeadersResult.HttpMethods.HttpGet
				End If
				' Всё хорошо, запускаю процессор
				m_Hosts.Item(www.HostName).ProcessRequest(state, objStream, www.VirtualPath, www.PhysicalDirectory)
			Else
				' HTTP/1.1 403 Forbidden
				' Этот тип файла не обслуживается
				state.StatusCode = 403
				state.WriteError(objStream, Error403, www.VirtualPath)
				Exit Do
			End If

ReadAllHeadersAgain:

			'Catch ex As Exception
			'	WriteLog(m_Lock, m_Logs, "Произошло исключение в OnSocketAccept " & ex.ToString)
			'End Try

			' TODO: освободить управляемое состояние (управляемые объекты).
			With state
				If .ZipStream IsNot Nothing Then
					.ZipStream.Close()
					.ZipStream.Dispose()
					'.ZipStream = Nothing
				End If
				If .MemoryStream IsNot Nothing Then
					.MemoryStream.Close()
					.MemoryStream.Dispose()
					'.MemoryStream = Nothing
				End If
			End With
		Loop While state.KeepAlive

		' TODO: освободить управляемое состояние (управляемые объекты).
		With state
			If .ZipStream IsNot Nothing Then
				.ZipStream.Close()
				.ZipStream.Dispose()
				.ZipStream = Nothing
			End If
			If .MemoryStream IsNot Nothing Then
				.MemoryStream.Close()
				.MemoryStream.Dispose()
				.MemoryStream = Nothing
			End If
		End With

		Thread.Sleep(5000)
		' Закрыть соединение
		objStream.Close()
		objStream.Dispose()
		objClient.Close()
		Debug.WriteLine("Поток {0} завершился", objState.Number)
	End Sub

	Public Sub WriteLog(ByVal lockObject As Object, ByVal logs As MemoryStream, ByVal s As String)
		Dim bytes = Encoding.UTF8.GetBytes(s)
		SyncLock lockObject
			logs.Write(bytes, 0, bytes.Length)
		End SyncLock
	End Sub

	Private Sub tmrTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles tmrLog.Elapsed
		' Нужен год месяц день
		Dim dNow As Date = Date.Now
		Dim sPath = Path.Combine(m_ExePath, String.Format("{0}-{1}-{2}.log", dNow.Year.ToString, dNow.Month.ToString, dNow.Day.ToString))
		SyncLock m_Lock
			If m_Logs.Length > 0 Then
				m_Logs.Seek(0, SeekOrigin.Begin)
				Using fs As New FileStream(sPath, FileMode.Append, FileAccess.Write)
					m_Logs.CopyTo(fs)
					fs.Close()
				End Using

				m_Logs.SetLength(0)
			End If
		End SyncLock
	End Sub

	''' <summary>
	''' Ищет символы 13 и 10 в буфере
	''' </summary>
	''' <param name="buffer">Буфер для поиска</param>
	''' <param name="Start">Начальный индекс элемента для поиска</param>
	''' <param name="BufferLength">Длина буфера</param>
	''' <returns>Возвращает индекс найденных байт. Если байты не найдены, возвращает -1</returns>
	''' <remarks></remarks>
	Public Function FindCrLf(ByVal buffer() As Byte, ByVal Start As Integer, ByVal BufferLength As Integer) As Integer
		For i As Integer = Start To BufferLength - 2 ' Минус 2 потому что два байта под CrLf
			If buffer(i) = 13 AndAlso buffer(i + 1) = 10 Then
				Return i
			End If
		Next
		Return -1
	End Function

	Public Sub RunServer()
		m_ContentTypes = New List(Of ContentType)
		m_AspNetProcessingFiles = New List(Of String)
		m_WebSites = New Dictionary(Of String, WebSite)
		m_Hosts = New Dictionary(Of String, Host)
		m_Logs = New MemoryStream(5 * 1024 * 1024)
		m_Lock = New Object
		m_ExePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly.Location)

		' Читать конфигурацию
		ReadConfiguration()

		' Создать среду хостинга для каждого сайта
		For Each kvp In m_WebSites
			If Not kvp.Value.IsMoved Then
				m_Hosts.Add(kvp.Key, CreateWorkerAppDomainWithHost(kvp.Value.VirtualPath, kvp.Value.PhysicalDirectory))
			End If
		Next

		Dim version4 = FileVersionInfo.GetVersionInfo(GetType(HttpRuntime).Module.FullyQualifiedName).FileVersion
		Dim version3 = version4.Substring(0, version4.LastIndexOf("."c))

		' Запустить слушатель
		If m_Any Then
			m_Listener = New TcpListener(IPAddress.Any, m_ServerPort)
		Else
			m_Listener = New TcpListener(IPAddress.Loopback, m_ServerPort)
		End If
		m_localEP = m_Listener.LocalEndpoint.ToString

		m_Listener.Start()
		tmrLog = New Timers.Timer(1000 * 60 * 15)
		tmrLog.Start()

		Do
			Dim objClient As TcpClient = Nothing
			Try
				objClient = m_Listener.AcceptTcpClient
			Catch ex As SocketException
				WriteLog(m_Lock, m_Logs, "Произошло исключение SocketException при попытке получения клиента AcceptTcpClient " & ex.ToString)
			Catch ex As Exception
				WriteLog(m_Lock, m_Logs, "Произошло исключение при попытке получения клиента AcceptTcpClient " & ex.ToString)
			End Try


			If objClient IsNot Nothing Then
				' Запустить приём соединений в другом потоке
				Dim t As New Thread(AddressOf OnSocketAccept) With {.IsBackground = True}
				Try
					t.Start(New StateObject With {.Client = objClient})
				Catch ex As Exception
					objClient.Close()
					WriteLog(m_Lock, m_Logs, "Произошло исключение при попытке создания потока " & ex.ToString)
				End Try
			End If
		Loop

	End Sub

End Module
