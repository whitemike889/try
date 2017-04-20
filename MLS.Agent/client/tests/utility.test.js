import parseQueryString from '../source/utility';

test('query string can have a leading ?', () => {
    var query = parseQueryString('?this=that')
    expect(query['this']).toBe('that');
});

test('query string can omit a leading ?', () => {
    var query = parseQueryString('this=that')
    expect(query['this']).toBe('that');
});

test('multiple values can be specified', () => {
    var query = parseQueryString('?this=that&the-other=thing')
    expect(query['this']).toBe('that');
    expect(query['the-other']).toBe('thing');
});

test('query string can be empty', () => {
    var query = parseQueryString('')
    expect(query['this']).toBe(undefined);
});

test('query string can consist only of a ?', () => {
    var query = parseQueryString('?')
    expect(query['this']).toBe(undefined);
});

test('values are not decoded', () => {
   var query = parseQueryString('?uri=https%3A%2F%2Fmicrosoft.com')
   expect(query['uri']).toBe('https%3A%2F%2Fmicrosoft.com');
});