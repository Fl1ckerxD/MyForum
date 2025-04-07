async function deletePost(postId, buttonElement) {
    if (!confirm('Вы уверены, что хотите удалить этот комментарий?')) {
        return;
    }

    try {
        const response = await fetch('/Admin/DeletePost?postId=' + postId, {
            method: 'POST'
        });

        if (response.ok) {
            const listItem = buttonElement.closest('li');
            listItem.remove();

            if (document.querySelectorAll('.glass-card ul li').length === 0) {
                document.querySelector('.glass-card ul').innerHTML = '<p>У пользователя нет комментариев.</p>'
            }
        }
        else {
            alert('Ошибка при удалении комментария');
        }
    }
    catch (error) {
        console.error('Error:', error);
    }
}

async function deleteTopic(topicId, buttonElement) {
    if (!confirm('Вы уверены, что хотите удалить этот пост?')) {
        return;
    }

    try {
        const respons = await fetch('/Admin/DeleteTopic?topicId=' + topicId, {
            method: 'POST'
        });
        if (respons.ok) {
            const listItem = buttonElement.closest('li');
            listItem.remove();

            if (document.querySelectorAll('.glass-card ul li').length === 0) {
                document.querySelector('.glass-card ul'.innerHTML = '<p>У пользователя нет постов.</p>');
            }
        }
        else {
            alert('Ошибка при удалении поста');
        }
    }
    catch (error) {
        console.error('Error:', error);
    }
}