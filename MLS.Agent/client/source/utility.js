
function parseQueryString(query) {
    if (!query) {
        return { };
    }

    if (query[0] == '?') {
        query = query.slice(1);
    }

    return query.split('&')
                .map(kv => kv.split('='))
                .reduce((hash, pair) => { 
                    hash[pair[0]] = pair[1];
                    return hash;
                }, {});
}

export default parseQueryString;