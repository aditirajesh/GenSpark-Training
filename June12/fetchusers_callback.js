

function getUsersWithCallback(callback) {
    console.log("CALLBACK: Starting to get users...");
    
    setTimeout(() => {
        console.log("users fetched");
        
        callback(dummyUsers);
    }, 2000);
}

function displayUsersCallback(users) {
    const resultDiv = document.getElementById("callback-result");

    resultDiv.innerHTML = "";

    users.forEach(user => {
        const userInfo = document.createElement("p");
        userInfo.textContent = `👤 ${user.name} - ${user.email}`;
        resultDiv.appendChild(userInfo);
    });
}


function useCallbackMethod() {
    console.log("calling callback method....");
    
    getUsersWithCallback(displayUsersCallback);
}