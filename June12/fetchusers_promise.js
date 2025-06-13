

function getUsersWithPromise() {
    console.log("🤝 PROMISE: Starting to get users...");
    
    return new Promise((resolve, reject) => {
        
        setTimeout(() => {
            
            const success = Math.random() > 0.2; 
            
            if (success) {
                console.log("Promise succeeded");
                resolve(dummyUsers);
            } else {
                console.log("Promise failed.");
                reject(new Error("Server error")); 
            }
        }, 2000);
    });
}

function usePromiseMethod() {
    console.log("starting promise method...");

    const resultDiv = document.getElementById("promise-result");
    resultDiv.innerHTML = "";

    getUsersWithPromise()
        .then(users => {
            console.log("Users:", users);

            users.forEach(user => {
                const userInfo = document.createElement("p");
                userInfo.textContent = `👤 ${user.name} - ${user.email}`;
                resultDiv.appendChild(userInfo);
            });
        })
        .catch(error => {
            console.log("Error occurred:", error.message);
            resultDiv.textContent = `❌ Error: ${error.message}`;
        });
}
