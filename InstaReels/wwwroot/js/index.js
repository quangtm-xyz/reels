(() => {
    const pageRoot = document.getElementById('index-page');
    if (!pageRoot) {
        return;
    }

    let localizedStrings = {};
    try {
        localizedStrings = JSON.parse(pageRoot.dataset.localized ?? '{}');
    } catch {
        localizedStrings = {};
    }

    const messages = {
        generalError: localizedStrings.errorGeneral ?? 'Something went wrong, please try again.',
        invalidLink: localizedStrings.errorInvalidLink ?? 'The Instagram link is invalid.',
        processing: localizedStrings.statusProcessing ?? 'Processing...',
    };

    const downloadButton = document.getElementById('downloadButton');
    const instagramLinkInput = document.getElementById('instagramLink');
    const errorAlert = document.getElementById('errorAlert');
    const errorMessage = document.getElementById('errorMessage');
    const endpoint = pageRoot.dataset.downloadEndpoint ?? '/api/download/reels';

    if (!downloadButton || !instagramLinkInput) {
        return;
    }

    const showError = (text) => {
        if (!errorAlert || !errorMessage) {
            window.alert(text);
            return;
        }

        errorMessage.textContent = text;
        errorAlert.classList.remove('hidden');
    };

    const hideError = () => {
        if (errorAlert) {
            errorAlert.classList.add('hidden');
        }
    };

    const handleDownload = async () => {
        const url = instagramLinkInput.value.trim();

        if (!url) {
            showError(messages.invalidLink);
            return;
        }

        downloadButton.disabled = true;
        const originalText = downloadButton.textContent;
        downloadButton.textContent = messages.processing;
        hideError();

        try {
            const response = await fetch(endpoint, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ url }),
            });

            const payload = await response.json();

            if (response.ok && payload.success && payload.downloadLink) {
                if (response.ok && payload.success && payload.downloadLink) {
                    // Cách 1: Mở tab mới + tự động tải (tốt nhất)
                    const newTab = window.open(payload.downloadLink, '_blank');
                    if (newTab) newTab.focus();

                    // Cách 2 (dự phòng nếu trình duyệt chặn popup): force download bằng <a> ẩn
                    const a = document.createElement('a');
                    a.href = payload.downloadLink;
                    a.download = ''; // để browser tự động tải về
                    a.target = '_blank';
                    a.rel = 'noopener';
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);

                    // Thông báo nhẹ cho người dùng biết đang tải
                    downloadButton.textContent = localizedStrings.downloaded ?? 'Đã tải!';
                    setTimeout(() => {
                        downloadButton.disabled = false;
                        downloadButton.textContent = originalText;
                    }, 2000);
                    return;
                }
            }

            const apiError = payload.error || payload.message || messages.generalError;
            showError(apiError);
        } catch (error) {
            console.error('download-error', error);
            showError(messages.generalError);
        } finally {
            downloadButton.disabled = false;
            downloadButton.textContent = originalText;
        }
    };

    downloadButton.addEventListener('click', handleDownload);
    instagramLinkInput.addEventListener('keypress', (event) => {
        if (event.key === 'Enter') {
            event.preventDefault();
            handleDownload();
        }
    });
})();



