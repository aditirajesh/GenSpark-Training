

function getUsersWithAsyncAwait() {
    console.log("async await: getting users...");
    
    return new Promise((resolve) => {
        setTimeout(() => {
            console.log("got users");
            resolve(dummyUsers);
        }, 2000);
    });
}

async function useAsyncAwaitMethod() {
    console.log("starting..");

    const resultDiv = document.getElementById("async-result");
    resultDiv.innerHTML = "";

    try {
        console.log("waiting for users ");
        const users = await getUsersWithAsyncAwait();

        console.log("Got users:", users);

        users.forEach(user => {
            const userInfo = document.createElement("p");
            userInfo.textContent = `👤 ${user.name} - ${user.email}`;
            resultDiv.appendChild(userInfo);
        });
    } catch (error) {
        console.log("Error:", error.message);
        resultDiv.textContent = `Error: ${error.message}`;
    }
}