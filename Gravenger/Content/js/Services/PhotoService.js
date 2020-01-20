function PhotoService(settings) {
    var self = this;
    self.apiUrl = (settings.rootUrl || "") + "api/";
    self.photoApiUrl = self.apiUrl + "photo";

    self.add = function (base64, mimeType) {
        return $.post(self.photoApiUrl, { base64: base64, mimeType: mimeType });
    }
    
    self.delete = function (fileName) {
        return $.ajax({
            url: self.photoApiUrl,
            type: 'DELETE',
            data: { fileName: fileName }
        });
    }
}