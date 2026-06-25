document.addEventListener('DOMContentLoaded', function () {
    const form = document.querySelector('form[action*="Register"]');
    if (!form) return;

    const radios = form.querySelectorAll('input[name="LegalType"]');
    const fieldIds = ['OrganizationName', 'Inn', 'OgrnOrOgrnip', 'LegalAddress'];

    function setVisible(input, visible) {
        if (!input) return;
        const label = input.previousElementSibling;
        if (label) label.style.display = visible ? '' : 'none';
        input.style.display = visible ? '' : 'none';
        input.disabled = !visible;
    }

    function update() {
        const selected = form.querySelector('input[name="LegalType"]:checked');
        const isBusiness = selected && selected.value === 'Business';
        fieldIds.forEach(id => setVisible(form.querySelector('#' + id), isBusiness));
    }

    radios.forEach(radio => radio.addEventListener('change', update));
    update();
});
