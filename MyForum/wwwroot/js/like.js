document.addEventListener("DOMContentLoaded", function () {
    // Находим все кнопки "Лайка"
    const likeButtons = document.querySelectorAll(".like-button");

    likeButtons.forEach(button => {
        button.addEventListener("click", async function () {
            const postId = button.getAttribute("data-post-id"); // Получаем ID поста

            try {
                // Отправляем AJAX-запрос
                const response = await fetch('/Posts/Like?postId=' + postId, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    }
                });

                if (!response.ok) {
                    throw new Error('Ошибка: ' + response.status);
                }

                const data = await response.json(); // Получаем ответ от сервера
                button.innerHTML = 'Понравилось ' + data.likesCount; // Обновляем текст кнопки
            }
            catch (error) {
                console.error("Ошибка при выполнении запроса:", error);
                alert("Произошла ошибка. Попробуйте позже.");
            }
        });
    });
});