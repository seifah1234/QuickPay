

(function () {

    "use strict";

    if (typeof signalR === "undefined") {
        console.error("SignalR library is not loaded.");
        return;
    }

    const notificationsList =
        document.getElementById("notificationsList");

    if (!notificationsList) {
        return;
    }


    const connection =
        new signalR.HubConnectionBuilder()
            .withUrl("/hubs/notifications")
            .withAutomaticReconnect()
            .build();


   

    connection.on("ReceiveNotification", function (data) {

        console.log("Notification received:", data);


       

        const emptyNotifications =
            document.getElementById("emptyNotifications");

        if (emptyNotifications) {
            emptyNotifications.remove();
        }


      

        const notification =
            document.createElement("div");

        notification.className =
            "list-group-item list-group-item-primary py-3 px-4 notification-item";


    

        notification.innerHTML = `
            <div class="d-flex justify-content-between align-items-start gap-3">

                <div class="d-flex gap-3 flex-grow-1">

                    <div class="qp-notification-icon qp-notification-unread">

                        <svg width="18"
                             height="18"
                             fill="currentColor"
                             viewBox="0 0 16 16">

                            <path d="M8 16a2 2 0 0 0 2-2H6a2 2 0 0 0 2 2zM8 1.918l-.797.161A4.002 4.002 0 0 0 4 6c0 .628-.134 2.197-.459 3.742-.16.767-.376 1.566-.663 2.258h10.244c-.287-.692-.502-1.49-.663-2.258C12.134 8.197 12 6.628 12 6a4.002 4.002 0 0 0-3.203-3.92L8 1.917zM14.22 12c.223.447.481.801.78 1H1c.299-.199.557-.553.78-1C2.68 10.2 3 9.073 3 8c0-2.754 1.781-5.05 4.258-5.74A2 2 0 1 1 9.742 2.26C12.219 2.95 14 5.246 14 8c0 1.073.32 2.2 1.22 4z"/>

                        </svg>

                    </div>


                    <div class="flex-grow-1">

                        <div class="d-flex justify-content-between align-items-start">

                            <div class="fw-semibold text-primary notification-type"></div>

                            <span class="qp-unread-dot"></span>

                        </div>


                        <div class="text-muted small mt-1 notification-message"></div>


                        <small class="text-muted d-block mt-1">

                            <svg width="12"
                                 height="12"
                                 fill="currentColor"
                                 viewBox="0 0 16 16"
                                 class="me-1">

                                <path d="M8 3.5a.5.5 0 0 0-1 0V9a.5.5 0 0 0 .252.434l3.5 2a.5.5 0 0 0 .496-.868L8 8.71V3.5z"/>

                                <path d="M8 16A8 8 0 1 0 8 0a8 8 0 0 0 0 16zm7-8A7 7 0 1 1 1 8a7 7 0 0 1 14 0z"/>

                            </svg>

                            Just now

                        </small>

                    </div>

                </div>

            </div>
        `;


        /*
         * Safely insert notification data
         */

        notification.querySelector(".notification-type")
            .textContent = data.type || "Notification";

        notification.querySelector(".notification-message")
            .textContent = data.message || "";


        /*
         * Add notification to the beginning
         */

        notificationsList.prepend(notification);


        /*
         * Update unread counter
         */

        const unreadBadge =
            document.getElementById("unreadBadge");

        const unreadCount =
            document.getElementById("unreadCount");


        if (unreadBadge && unreadCount) {

            let count =
                parseInt(unreadCount.textContent) || 0;

            count++;

            unreadCount.textContent = count;

            unreadBadge.classList.remove("d-none");
        }

    });


    /*
     * Start SignalR connection
     */

    async function startConnection() {

        try {

            await connection.start();

            console.log(
                "SignalR Connected:",
                connection.connectionId
            );

        }
        catch (error) {

            console.error(
                "SignalR connection failed:",
                error
            );

          

            setTimeout(startConnection, 5000);
        }
    }


    startConnection();


   

    connection.onreconnecting(function (error) {

        console.warn(
            "SignalR reconnecting...",
            error
        );

    });


    connection.onreconnected(function (connectionId) {

        console.log(
            "SignalR reconnected:",
            connectionId
        );

    });


    connection.onclose(function (error) {

        console.error(
            "SignalR connection closed.",
            error
        );

    });


})();