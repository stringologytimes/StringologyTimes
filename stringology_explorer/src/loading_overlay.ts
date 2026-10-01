export function showLoading(message: string = "Loading..."): void {
  const loadingOverlay = document.getElementById("loading-overlay");
  if (loadingOverlay) {
    const loadingText = loadingOverlay.querySelector(".loading-text");
    if (loadingText) {
      loadingText.textContent = message;
    }
    loadingOverlay.classList.add("show");
  }
}

export function hideLoading(): void {
  const loadingOverlay = document.getElementById("loading-overlay");
  if (loadingOverlay) {
    loadingOverlay.classList.remove("show");
  }
}

/** Let the browser paint the loading overlay before synchronous heavy work. */
export function yieldForPaint(): Promise<void> {
  return new Promise((resolve) => {
    requestAnimationFrame(() => {
      requestAnimationFrame(() => resolve());
    });
  });
}
