function CardService(settings) {
    var self = this;
    self.apiUrl = (settings.rootUrl || "") + "api/";

    self.get = function (cardID) {
        //TODO: handle errors appropriately
        return $.get(self.apiUrl + "card/" + cardID);
    }

    self.getByCardIDAndUsername = function (cardID, username) {
        //TODO: handle errors appropriately
        return $.get(self.apiUrl + "card/" + cardID + "/" + username);
    }
    
    self.getAll = function () {
        //TODO: handle errors appropriately
        return $.get(self.apiUrl + "card");
    }

    self.addPostcard = function (cardID) {
        //TODO: handle errors appropriately
        return $.post(self.apiUrl + "account/addpostcard/" + cardID);
    }

    self.removePostcard = function (cardID) {
        //TODO: handle errors appropriately
        return $.ajax({
            url: self.apiUrl + "account/removePostcard/" + cardID,
            type: 'DELETE'
        });
    }

    self.search = function (criteria, settings) {
        //TODO: handle errors appropriately
        var global = settings && settings.global;
        
        return $.get({ url: self.apiUrl + 'card/search/' + criteria, global: global });
    }

    self.likePostcard = function (postcardID) {
        return $.post(self.apiUrl + "account/likepostcard/" + postcardID);
    }

    self.unlikePostcard = function (postcardID) {
        return $.ajax({
            url: self.apiUrl + "account/removePostcardlike/" + postcardID,
            type: 'DELETE'
        });
    }
}