(function () {
    var searchInput = document.getElementById('recipientSearch');
    var resultsBox = document.getElementById('recipientResults');
    var hiddenIdInput = document.getElementById('ToAccountIdInput');

    if (!searchInput || !resultsBox || !hiddenIdInput) {
        return;
    }

    var debounceTimer = null;

    function clearResults() {
        resultsBox.innerHTML = '';
    }

    function renderResults(items) {
        clearResults();

        if (!items.length) {
            return;
        }

        items.forEach(function (item) {
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'list-group-item list-group-item-action';
            btn.textContent = item.displayName;

            btn.addEventListener('click', function () {
                hiddenIdInput.value = item.id;
                searchInput.value = item.displayName;
                clearResults();
            });

            resultsBox.appendChild(btn);
        });
    }

    searchInput.addEventListener('input', function () {
        // Any manual edit invalidates the previously selected recipient.
        hiddenIdInput.value = '0';

        var query = searchInput.value.trim();

        if (debounceTimer) {
            clearTimeout(debounceTimer);
        }

        if (query.length < 2) {
            clearResults();
            return;
        }

        debounceTimer = setTimeout(function () {
            fetch('/Transfer/SearchRecipients?query=' + encodeURIComponent(query))
                .then(function (response) {
                    return response.ok ? response.json() : [];
                })
                .then(renderResults)
                .catch(function () {
                    clearResults();
                });
        }, 300);
    });

    document.addEventListener('click', function (event) {
        if (!resultsBox.contains(event.target) && event.target !== searchInput) {
            clearResults();
        }
    });
})();
