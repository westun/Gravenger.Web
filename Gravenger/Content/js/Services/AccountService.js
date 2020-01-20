function AccountService(settings) {
    var self = this;
    self.apiUrl = (settings.rootUrl || "") + "api/";

    self.get = function (accountID) {
        //TODO: handle errors appropriately
        return $.get(self.apiUrl + "account/" + accountID);
    }
    
    self.follow = function (followeeid) {
        //TODO: handle errors appropriately
        return $.post(self.apiUrl + "following", { followeeid: followeeid });
    }

    self.unfollow = function (followeeid) {
        //TODO: handle errors appropriately
        return $.ajax({
            url: self.apiUrl + "following",
            data: { followeeid: followeeid },
            type: 'DELETE'
        });
    }

    self.getAllByCardID = function (cardID) {
        //TODO: handle errors appropriately
        return $.get(self.apiUrl + "card/" + cardID + "/postcards/accounts");
    }

    self.getAllThatLikePostcard = function (postcardID) {
        //TODO: handle errors appropriately
        return $.get(self.apiUrl + "postcard/" + postcardID + "/postcardlikes/accounts");
    }
}