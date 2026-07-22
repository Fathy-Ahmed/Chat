export function createStorageService(storage, key) {
  function getAuth() {
    const rawValue = storage.getItem(key);
    if (!rawValue) return null;

    try {
      return JSON.parse(rawValue);
    } catch {
      storage.removeItem(key);
      return null;
    }
  }

  return {
    getAuth,
    saveAuth(auth) {
      storage.setItem(key, JSON.stringify(auth));
    },
    clearAuth() {
      storage.removeItem(key);
    },
  };
}
