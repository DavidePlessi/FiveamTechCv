export function formatDate(dateValue?: string) {
    if (!dateValue) return '';
    // Use ISO string slice to get YYYY-MM-DD.
    // Assuming backend sends correct ISO format which parses correctly.
    try {
        return new Date(dateValue).toISOString().slice(0, 10);
    } catch (e) {
        return dateValue;
    }
}