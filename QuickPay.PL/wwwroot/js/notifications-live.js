// Connects to /hubs/notifications and prepends any incoming
// notification to the top of the list live, without a page reload.
// The connection is authenticated via the same "access_token" HttpOnly
// cookie the rest of the app uses (SignalR sends cookies automatically
// on same-origin requests), so the server-side hub already knows which
// user this is - no userId is passed here.

(function () {
    if (typeof signalR === 'undefined') {
        return;
    }

    var connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/notifications')
        .withAutomaticReconnect()
        .build();

    connection.on('ReceiveNotification', function (payload) {
        var list = document.querySelector('.list-group');
        if (!list) {
            return;
        }

        var item = document.createElement('div');
        item.className = 'list-group-item list-group-item-primary';
        item.innerHTML =
            '<div class="fw-semibold"></div><div></div>';
        item.querySelector('.fw-semibold').textContent = payload.type;
        item.querySelector('div:last-child').textContent = payload.message;

        list.prepend(item);
    });

    connection.start().catch(function (err) {
        console.error('SignalR connection failed:', err);
    });
})();
