document.addEventListener("DOMContentLoaded", function () {
    // Hiệu ứng input focus
    const inputs = document.querySelectorAll(".form-floating input");

    inputs.forEach(input => {
        input.addEventListener("focus", function () {
            this.parentElement.classList.add("focused");
        });

        input.addEventListener("blur", function () {
            if (!this.value) {
                this.parentElement.classList.remove("focused");
            }
        });
    });

    // Hiển thị / Ẩn mật khẩu
    const passwordField = document.querySelector("input[type='password']");
    if (passwordField) {
        let toggleBtn = document.createElement("span");
        toggleBtn.innerHTML = "👁";
        toggleBtn.classList.add("toggle-password");
        passwordField.parentElement.appendChild(toggleBtn);

        toggleBtn.addEventListener("click", function () {
            if (passwordField.type === "password") {
                passwordField.type = "text";
                this.innerHTML = "🙈"; // Icon thay đổi khi ẩn mật khẩu
            } else {
                passwordField.type = "password";
                this.innerHTML = "👁";
            }
        });
    }
});
document.addEventListener("DOMContentLoaded", function () {
    const inputFields = document.querySelectorAll(".form-floating input");

    inputFields.forEach(input => {
        input.addEventListener("input", function (event) {
            createFruitEffect(event);
        });
    });

    function createFruitEffect(event) {
        const fruitIcons = ["🍎", "🍌", "🍇", "🍉", "🍒", "🍍", "🥑", "🍊"];
        const fruit = document.createElement("span");
        fruit.innerText = fruitIcons[Math.floor(Math.random() * fruitIcons.length)];
        fruit.classList.add("fruit-effect");

        // Lấy vị trí con trỏ trong input
        const input = event.target;
        const { selectionStart } = input;
        const inputRect = input.getBoundingClientRect();
        const charWidth = 8; // Kích thước trung bình của 1 ký tự (tùy vào font)

        // Xác định vị trí tương đối của con trỏ chữ
        const cursorX = inputRect.left + selectionStart * charWidth + 15;
        const cursorY = inputRect.top + inputRect.height / 2;

        // Đặt vị trí icon gần con trỏ chữ
        fruit.style.left = `${cursorX}px`;
        fruit.style.top = `${cursorY}px`;

        document.body.appendChild(fruit);

        // Tạo hiệu ứng bay lên & biến mất
        setTimeout(() => fruit.remove(), 1000);
    }
});
document.addEventListener("DOMContentLoaded", function () {
    const fruitIcons = ["🍎", "🍌", "🍇", "🍉", "🍒", "🍍", "🥑", "🍊"];

    document.addEventListener("click", function (event) {
        createFruitEffect(event.clientX, event.clientY);
    });

    function createFruitEffect(x, y) {
        const fruit = document.createElement("span");
        fruit.innerText = fruitIcons[Math.floor(Math.random() * fruitIcons.length)];
        fruit.classList.add("fruit-effect");

        fruit.style.left = `${x}px`;
        fruit.style.top = `${y}px`;

        document.body.appendChild(fruit);

        // Tạo hiệu ứng bay lên & biến mất
        setTimeout(() => fruit.remove(), 1000);
    }
});
