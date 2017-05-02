''' <summary>
''' Миме‐тип файла
''' </summary>
<Serializable()> Public Class ContentType

	''' <summary>
	''' Миме‐типы
	''' </summary>
	Public Enum ContentTypes
		None

		ImageJpeg
		ImageGif
		ImagePng
		ImageTiff
		ImageIco
		ImageSvg

		TextCss
		ApplicationXml
		ApplicationXmlXslt
		TextPlain
		TextHtml
		ApplicationXhtml
		ApplicationAtom
		RssXml
		ApplicationJavascript

		Application7z
		ApplicationRar
		ApplicationZip
		ApplicationGzip
		ApplicationXCompressed

		ApplicationRtf
		ApplicationPdf
		ApplicationOpenDocumentText
		ApplicationOpenDocumentTextTemplate
		ApplicationOpenDocumentGraphics
		ApplicationOpenDocumentGraphicsTemplate
		ApplicationOpenDocumentPresentation
		ApplicationOpenDocumentPresentationTemplate
		ApplicationOpenDocumentSpreadsheet
		ApplicationOpenDocumentSpreadsheetTemplate
		ApplicationOpenDocumentChart
		ApplicationOpenDocumentChartTemplate
		ApplicationOpenDocumentImage
		ApplicationOpenDocumentImageTemplate
		ApplicationOpenDocumentFormula
		ApplicationOpenDocumentFormulaTemplate
		ApplicationOpenDocumentMaster
		ApplicationOpenDocumentWeb

		VideoMpeg
		VideoOgg
		VideoMp4
		VideoWebm

		ApplicationOctetStream

		ApplicationFlash
		AudioRealaudio
		ApplicationCertx509
	End Enum

	Public Shared Function GetExtension(ByVal ct As ContentTypes) As String
		Select Case ct
			Case ContentTypes.None
				Return String.Empty
			Case ContentTypes.ImageJpeg
				Return ".jpg"
			Case ContentTypes.ImageGif
				Return ".gif"
			Case ContentTypes.ImagePng
				Return ".png"
			Case ContentTypes.ImageTiff
				Return ".tif"
			Case ContentTypes.ImageIco
				Return ".ico"
			Case ContentTypes.ImageSvg
				Return ".svg"
			Case ContentTypes.TextCss
				Return ".css"
			Case ContentTypes.ApplicationXml
				Return ".xml"
			Case ContentTypes.ApplicationXmlXslt
				Return ".xslt"
			Case ContentTypes.TextPlain
				Return ".txt"
			Case ContentTypes.TextHtml
				Return ".htm"
			Case ContentTypes.ApplicationXhtml
				Return ".xhtml"
			Case ContentTypes.ApplicationAtom
				Return ".atom"
			Case ContentTypes.RssXml
				Return ".rss"
			Case ContentTypes.ApplicationJavascript
				Return ".js"
			Case ContentTypes.Application7z
				Return ".7z"
			Case ContentTypes.ApplicationRar
				Return ".rar"
			Case ContentTypes.ApplicationZip
				Return ".zip"
			Case ContentTypes.ApplicationGzip
				Return ".gz"
			Case ContentTypes.ApplicationXCompressed
				Return ".tgz"
			Case ContentTypes.ApplicationRtf
				Return ".rtf"
			Case ContentTypes.ApplicationPdf
				Return ".pdf"
			Case ContentTypes.ApplicationOpenDocumentText
				Return ".odt"
			Case ContentTypes.ApplicationOpenDocumentTextTemplate
				Return ".ott"
			Case ContentTypes.ApplicationOpenDocumentGraphics
				Return ".odg"
			Case ContentTypes.ApplicationOpenDocumentGraphicsTemplate
				Return ".otg"
			Case ContentTypes.ApplicationOpenDocumentPresentation
				Return ".odp"
			Case ContentTypes.ApplicationOpenDocumentPresentationTemplate
				Return ".otp"
			Case ContentTypes.ApplicationOpenDocumentSpreadsheet
				Return ".ods"
			Case ContentTypes.ApplicationOpenDocumentSpreadsheetTemplate
				Return ".ots"
			Case ContentTypes.ApplicationOpenDocumentChart
				Return ".odc"
			Case ContentTypes.ApplicationOpenDocumentChartTemplate
				Return ".otc"
			Case ContentTypes.ApplicationOpenDocumentImage
				Return ".odi"
			Case ContentTypes.ApplicationOpenDocumentImageTemplate
				Return ".oti"
			Case ContentTypes.ApplicationOpenDocumentFormula
				Return ".odf"
			Case ContentTypes.ApplicationOpenDocumentFormulaTemplate
				Return ".otf"
			Case ContentTypes.ApplicationOpenDocumentMaster
				Return ".odm"
			Case ContentTypes.ApplicationOpenDocumentWeb
				Return ".oth"
			Case ContentTypes.VideoMpeg
				Return ".mpg"
			Case ContentTypes.VideoOgg
				Return ".ogv"
			Case ContentTypes.VideoMp4
				Return ".mp4"
			Case ContentTypes.VideoWebm
				Return ".webm"
			Case ContentTypes.ApplicationOctetStream
				Return ".bin"
			Case ContentTypes.ApplicationFlash
				Return ".sfw"
			Case ContentTypes.AudioRealaudio
				Return ".ram"
			Case ContentTypes.ApplicationCertx509
				Return ".cer"
			Case Else
				Return String.Empty
		End Select
	End Function

	Public Shared Function GetContentType(ByVal ext As String) As ContentTypes
		Select Case ext
			Case ".jpg", ".jpe", ".jpeg"
				Return ContentTypes.ImageJpeg
			Case ".gif"
				Return ContentTypes.ImageGif
			Case ".png"
				Return ContentTypes.ImagePng
			Case ".tif", ".tiff"
				Return ContentTypes.ImageTiff
			Case ".ico"
				Return ContentTypes.ImageIco
			Case ".css"
				Return ContentTypes.TextCss
			Case ".xml"
				Return ContentTypes.ApplicationXml
			Case ".xsl", ".xslt"
				Return ContentTypes.ApplicationXmlXslt
			Case ".txt"
				Return ContentTypes.TextPlain
			Case ".htm", ".html", ".shtml"
				Return ContentTypes.TextHtml
			Case ".xhtml"
				Return ContentTypes.ApplicationXhtml
			Case ".atom"
				Return ContentTypes.ApplicationAtom
			Case ".rss"
				Return ContentTypes.RssXml
			Case ".js"
				Return ContentTypes.ApplicationJavascript
			Case ".7z"
				Return ContentTypes.Application7z
			Case ".rar"
				Return ContentTypes.ApplicationRar
			Case ".zip"
				Return ContentTypes.ApplicationZip
			Case ".gz"
				Return ContentTypes.ApplicationGzip
			Case ".tgz"
				Return ContentTypes.ApplicationXCompressed
			Case ".rtf"
				Return ContentTypes.ApplicationRtf
			Case ".mpg", ".mpeg"
				Return ContentTypes.VideoMpeg
			Case ".ogv"
				Return ContentTypes.VideoOgg
			Case ".mp4"
				Return ContentTypes.VideoMp4
			Case ".webm"
				Return ContentTypes.VideoWebm
			Case ".bin", ".exe", ".dll", ".deb", ".dmg", ".eot", ".iso", ".img", ".msi", ".msp", ".msm"
				Return ContentTypes.ApplicationOctetStream
			Case ".sfw"
				Return ContentTypes.ApplicationFlash
			Case ".ram"
				Return ContentTypes.AudioRealaudio
			Case ".crt", ".cer"
				Return ContentTypes.ApplicationCertx509
				'<add extension=".rtf" contentType="application/rtf" isText="True" />
				'<add extension=".pdf" contentType="application/pdf" isText="False" />
				'<add extension=".odt" contentType="application/vnd.oasis.opendocument.text" isText="False" />
				'<add extension=".ott" contentType="application/vnd.oasis.opendocument.text-template" isText="False" />
				'<add extension=".odg" contentType="application/vnd.oasis.opendocument.graphics" isText="False" />
				'<add extension=".otg" contentType="application/vnd.oasis.opendocument.graphics-template" isText="False" />
				'<add extension=".odp" contentType="application/vnd.oasis.opendocument.presentation" isText="False" />
				'<add extension=".otp" contentType="application/vnd.oasis.opendocument.presentation-template" isText="False" />
				'<add extension=".ods" contentType="application/vnd.oasis.opendocument.spreadsheet" isText="False" />
				'<add extension=".ots" contentType="application/vnd.oasis.opendocument.spreadsheet-template" isText="False" />
				'<add extension=".odc" contentType="application/vnd.oasis.opendocument.chart" isText="False" />
				'<add extension=".otc" contentType="application/vnd.oasis.opendocument.chart-template" isText="False" />
				'<add extension=".odi" contentType="application/vnd.oasis.opendocument.image" isText="False" />
				'<add extension=".oti" contentType="application/vnd.oasis.opendocument.image-template" isText="False" />
				'<add extension=".odf" contentType="application/vnd.oasis.opendocument.formula" isText="False" />
				'<add extension=".otf" contentType="application/vnd.oasis.opendocument.formula-template" isText="False" />
				'<add extension=".odm" contentType="application/vnd.oasis.opendocument.text-master" isText="False" />
				'<add extension=".oth" contentType="application/vnd.oasis.opendocument.text-web" isText="False" />
			Case Else
				Return ContentTypes.None
		End Select
	End Function

	Public Property Extension As String
	Public Property ContentType As String
	Public Property IsTextFormat As Boolean
End Class
