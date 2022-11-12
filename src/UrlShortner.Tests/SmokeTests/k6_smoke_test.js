import http from 'k6/http';
import { check, group, sleep, fail } from 'k6';

export const options = {
    vus: 5, 
    duration: '15s',
};

const BASE_URL = 'http://host.docker.internal:8080';
const URLS = ['http://www.google.com', 'https://www.google.com', 'https://www.google.com/search?q=url+shortener&oq=google+u&aqs=chrome.0.69i59j69i60l3j0j69i57.1069j0j7&sourceid=chrome&ie=UTF-8'];
const slugs = [];

export default () => {    
    const shortenRes = http.post(`${BASE_URL}/shorten`, JSON.stringify({ LongUrl: URLS[Math.floor(Math.random() * URLS.length)] }), { headers: { 'Content-Type': 'application/json' } });
    check(shortenRes, {
        'shortened': (resp) => {
            const slug = resp.json('slug');
			slugs.push(slug);
            return slug !== ''
        },
    });

    for (let i = 0; i < 5; i++) {
        const res = http.get(`${BASE_URL}/${slugs[Math.floor(Math.random() * slugs.length)]}`, { redirects: 0 });
        check(res, { 'direct 302': (r) => r.status === 302 });
    }
};