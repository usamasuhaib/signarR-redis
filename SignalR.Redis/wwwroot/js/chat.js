"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (user, message) {
    displayMessage(user, message);
});

// Convert timestamp to local time string (time only)
function formatTime(timestamp) {
    const date = new Date(timestamp);
    return date.toLocaleTimeString(); // Returns only the time portion
}

function displayMessage(user, message, timeStamp) {

    var userName = document.createElement("h5");
    userName.className = "m-1"
    userName.innerText = user;

    var Msg = document.createElement("p");
    Msg.className = "m-2";
    Msg.innerText = message;

    var time = document.createElement("span");

    var CardBody = document.createElement("div");
    CardBody.className = "card-body p-2";
    CardBody.style.textAlign = "left";

    CardBody.appendChild(userName);
    CardBody.appendChild(Msg);
    CardBody.appendChild(time);

    var MessageCard = document.createElement("div");
    MessageCard.className = "card bg-light text-dark m-md-2";
    MessageCard.style.width = "fit-content";


    MessageCard.appendChild(CardBody);

    document.getElementById("MessagContainer").appendChild(MessageCard);



}


// Function to retrieve and display chat messages from cache
function retrieveAndDisplayMessages() {
    fetch("/Home/GetChatMessages")
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(messages => {
            // Display each message
            messages.forEach(message => {
                displayMessage(message.userName, message.message, message.timeStamp);
            });
        })
        .catch(error => {
            console.error('Error retrieving chat messages:', error);
        });
}

// Start the connection
// Start the connection and retrieve existing messages
connection.start()
    .then(function () {
        document.getElementById("sendButton").disabled = false;
        // Retrieve and display existing chat messages from cache
        retrieveAndDisplayMessages();
    })
    .catch(function (err) {
        console.error('Error starting SignalR connection:', err.toString());
    });


document.getElementById("sendButton").addEventListener("click", function (event) {

    // Call sendMessage function
    sendMessage();
    event.preventDefault();
});

// Add event listener to input field for keydown event
document.getElementById("messageInput").addEventListener("keydown", function (event) {
    // Check if Enter key is pressed (key code 13)
    if (event.keyCode === 13) {
        // Prevent default behavior (form submission)
        event.preventDefault();
        // Call sendMessage function
        sendMessage();
    }
});


//send mesage function
function sendMessage() {
    var message = document.getElementById("messageInput").value;
    if (message.trim() !== "") { // Ensure message is not empty
        // Send message to the server
        connection.invoke("SendMessage", message)
            .then(function () {
                // Clear the input field after successful message sending
                document.getElementById("messageInput").value = "";
            })
            .catch(function (err) {
                console.error('Error sending message:', err.toString());
            });
    }
}

// Retrieve and display existing chat messages from cache when the page initially loads
//retrieveAndDisplayMessages();