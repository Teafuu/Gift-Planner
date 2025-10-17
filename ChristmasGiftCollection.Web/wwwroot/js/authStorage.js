// Authentication storage using localStorage
// Stores remembered member ID with 30-day expiration

window.authStorage = {
    // Save member ID to localStorage with expiration
    saveRememberedMember: function (memberId) {
        const expirationDate = new Date();
        expirationDate.setDate(expirationDate.getDate() + 30); // 30 days from now

        const data = {
            memberId: memberId,
            expiration: expirationDate.toISOString()
        };

        localStorage.setItem('rememberedMember', JSON.stringify(data));
    },

    // Get remembered member ID if not expired
    getRememberedMember: function () {
        const stored = localStorage.getItem('rememberedMember');

        if (!stored) {
            return null;
        }

        try {
            const data = JSON.parse(stored);
            const expirationDate = new Date(data.expiration);
            const now = new Date();

            // Check if expired
            if (now > expirationDate) {
                localStorage.removeItem('rememberedMember');
                return null;
            }

            return data.memberId;
        } catch (e) {
            // Invalid data, remove it
            localStorage.removeItem('rememberedMember');
            return null;
        }
    },

    // Clear remembered member
    clearRememberedMember: function () {
        localStorage.removeItem('rememberedMember');
    }
};
