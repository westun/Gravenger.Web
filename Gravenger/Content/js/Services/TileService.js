function TileService(settings) {
    var self = this;
    self.apiUrl = (settings.rootUrl || "") + "api/";
    
    self.get = function (tileID) {
        //TODO: handle errors appropriately
        return $.get(self.apiUrl + "tile");
    }

    self.addAccountTile = function (tile) {
        return $.ajax({
            url: self.apiUrl + "accounttile",
            type: 'POST',
            data: tile            
        });
    }

    self.updateAccountTile = function (tile) {
        //TODO: handle errors appropriately
        //var def = $.Deferred();
        return $.ajax({
            url: self.apiUrl + "accounttile",
            type: 'PUT',
            data: tile
            //TODO: is the correct approach for error handling?
            //success: function (data) {
            //    def.resolve(data);
            //},
            //error: function (data) {
            //    def.reject(data);
            //}
        });
        //return def.promise();
    }

    self.deleteAccountTile = function (tileID) {
        //TODO: handle errors appropriately
        return $.ajax({
            url: self.apiUrl + "accounttile",
            type: 'DELETE',
            data: { tileID: tileID }
        });
    }

    self.likeAccountTile = function (accountTileID) {
        return $.post(self.apiUrl + "account/likeAccountTile/" + accountTileID);
    }

    self.unlikeAccountTile = function (accountTileID) {
        return $.ajax({
            url: self.apiUrl + "account/removeAccountTilelike/" + accountTileID,
            type: 'DELETE'
        });
    }
}