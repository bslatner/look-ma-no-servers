export function compareStrings(s1: string, s2: string) {
    const c1 = s1.toUpperCase();
    const c2 = s2.toUpperCase();
    if (c1 === c2) return 0;
    if (c1 > c2) return 1;
    return -1;
}

export function serializeForm(form: JQuery) {
    const values = form.serializeArray();
    var result = [];
    for (let i = 0; i < values.length; i++) {
        result[values[i].name] = values[i].value;
    }
    return result;
}