async function addComment(topicId, categoryName, buttonElement) {
    const textarea = document.getElementById("commentTextarea");
    const content = textarea.value.trim();
    const errorElement = document.getElementById("commentError");

    // Очистка предыдущих ошибок
    errorElement.textContent = "";

    // Валидация на стороне клиента
    if (!content) {
        errorElement.textContent = "Комментарий не может быть пустым.";
        return;
    }
    if (content.length > 15000) {
        errorElement.textContent = "Длина комментария не должна превышать 15000 символов.";
        return;
    }

    try {
        // Отправляем AJAX-запрос
        const response = await fetch('/Posts/Comment', {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                topicId: topicId,
                categoryName: categoryName,
                content: content
            })
        });

        if (!response.ok) {
            throw new Error('Ошибка: ' + response.status);
        }

        const data = await response.json();

        // Очищаем поле ввода
        textarea.value = "";

        // Добавляем новый комментарий в список
        const commentList = document.querySelector(".navbar-nav.flex-grow-1");
        const newComment = createCommentElement(data);
        commentList.appendChild(newComment);
    }
    catch (error) {
        console.error("Ошибка при отправке комментария:", error);
        errorElement.textContent = "Произошла ошибка. Попробуйте позже.";
    }
}

function createCommentElement(commentData) {
    const li = document.createElement("li");
    li.innerHTML = `
        <strong>${commentData.username}</strong>
        <label class="welcome-text topic-time">${commentData.createdAt}</label>
        <button type="button" class="button-transparent" onclick="deletePost(${commentData.id}, this)">
            <svg class="delete-button-circle" width="24" height="24" viewBox="0 0 24 24">
                <circle cx="12" cy="12" r="10" />
                <path d="M15 9L9 15M9 9L15 15" stroke="#ffff" stroke-width="2" stroke-linecap="round" />
            </svg>
        </button>
        <p>${commentData.content}</p>
        <button type="button" class="like-button submit-button" data-post-id="${commentData.id}">
            Понравилось 0
        </button>
    `;
    return li;
}