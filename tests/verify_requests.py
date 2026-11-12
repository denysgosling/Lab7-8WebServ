from datetime import date, timedelta
from html.parser import HTMLParser
import http.cookiejar
import json
from pathlib import Path
import urllib.error
import urllib.parse
import urllib.request

BASE = "http://127.0.0.1:5107"
client = urllib.request.build_opener(
    urllib.request.ProxyHandler({}),
    urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
checks = []


class FormPage(HTMLParser):
    def __init__(self, html):
        super().__init__()
        self.errors = set()
        self.inputs = {}
        self.feed(html)

    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if "data-valmsg-for" in attrs and "field-validation-error" in attrs.get("class", ""):
            self.errors.add(attrs["data-valmsg-for"])
        if tag == "input" and "name" in attrs:
            self.inputs[attrs["name"]] = attrs.get("value", "")


def get(path):
    return client.open(BASE + path).read().decode()


def submit(values, valid=False, error=None):
    token = FormPage(get("/Requests")).inputs["__RequestVerificationToken"]
    response = client.open(urllib.request.Request(BASE + "/Requests", urllib.parse.urlencode(
        {**values, "__RequestVerificationToken": token}).encode()))
    html = response.read().decode()
    parsed = FormPage(html)
    if valid:
        assert "alert-success" in html and not parsed.errors, html
        assert parsed.inputs.get("FirstName", "") == "", "PRG must return a clean form"
    else:
        assert "validation-summary-errors" in html, html
        if error:
            assert error in parsed.errors, (error, parsed.errors)
    return html, parsed


def check(name, values, error=None, valid=False):
    submit(values, valid=valid, error=error)
    checks.append(name)


valid = dict(FirstName="Денис", LastName="Карпов", Email="denys@example.com",
             Phone="+380671234567", Age="20", Subject="Новий вебсайт", Category="Development",
             Message="Потрібно створити сайт з каталогом матеріалів та формою зворотного зв’язку.",
             Website="https://example.com", PreferredDate=(date.today() + timedelta(days=7)).isoformat())
for path in ["/", "/Mail", "/Lifetimes", "/Journal", "/Files", "/Requests",
             "/css/site.css", "/lib/bootstrap/dist/css/bootstrap.min.css"]:
    assert client.open(BASE + path).status == 200, path
    checks.append("GET " + path)
html = get("/Requests")
for name in valid:
    assert f'name="{name}"' in html, name
checks.append("all ten form fields")
html, parsed = submit({})
required = {"FirstName", "LastName", "Email", "Age", "Subject", "Category", "Message"}
assert required <= parsed.errors and not ({"Phone", "Website", "PreferredDate"} & parsed.errors)
checks.append("empty form and nullable optional fields")
for field in sorted(required):
    check("required " + field, {**valid, field: ""}, error=field)
invalid = {
    "FirstName": ["А", "123", "А" * 41],
    "LastName": ["К", "Karpov123", "К" * 41],
    "Email": ["bad-email", "a" * 250 + "@example.com"],
    "Phone": ["123", "+38067123456x"],
    "Age": ["15", "101", "abc"],
    "Subject": ["Тест", "Т" * 101],
    "Category": ["Unknown"],
    "Message": ["Коротко", "О" * 2001],
    "Website": ["example.com", "javascript:alert(1)", "https://example.com/" + "a" * 201],
    "PreferredDate": [(date.today()-timedelta(days=1)).isoformat(),
                      (date.today()+timedelta(days=91)).isoformat(), "not-a-date"]
}
for field, values in invalid.items():
    for i, value in enumerate(values, 1):
        check(f"invalid {field} {i}", {**valid, field: value}, error=field)
all_bad = {**valid, **{field: values[0] for field, values in invalid.items()}}
_, parsed = submit(all_bad)
assert set(valid) <= parsed.errors, parsed.errors
checks.append("all ten field errors together")
html, parsed = submit({**valid, "Email": "bad-email"})
assert parsed.inputs["FirstName"] == valid["FirstName"] and parsed.inputs["Email"] == "bad-email"
checks.append("retain posted values on errors")
check("valid all fields", valid, valid=True)
check("valid nullable fields omitted", {**valid, "Phone": "", "Website": "", "PreferredDate": ""}, valid=True)
check("minimum boundaries", {**valid, "FirstName": "Ія", "LastName": "Лі", "Age": "16",
      "Subject": "Сайт!", "Message": "А" * 20, "PreferredDate": date.today().isoformat()}, valid=True)
check("maximum boundaries", {**valid, "FirstName": "А" * 40, "LastName": "К" * 40, "Age": "100",
      "Subject": "Т" * 100, "Message": "О" * 2000,
      "PreferredDate": (date.today()+timedelta(days=90)).isoformat()}, valid=True)
check("valid apostrophe and hyphen", {**valid, "FirstName": "Мар’яна", "LastName": "Коваль-Петренко"}, valid=True)
try:
    client.open(urllib.request.Request(BASE + "/Requests", urllib.parse.urlencode(valid).encode()))
    raise AssertionError("Missing CSRF token must be rejected")
except urllib.error.HTTPError as error:
    assert error.code == 400
checks.append("reject missing antiforgery token")
token = FormPage(get("/Requests")).inputs["__RequestVerificationToken"]
client.open(urllib.request.Request(BASE + "/Home/SetLanguage", urllib.parse.urlencode({
    "culture": "en-US", "returnUrl": "/Requests", "__RequestVerificationToken": token}).encode())).read()
html = get("/Requests")
assert "Tell us about your idea." in html
html, _ = submit({**valid, "Email": "wrong"})
assert "Enter a valid email address." in html and "RequestEmailFormat" not in html
checks.append("English interface and validation messages")
Path("evidence").mkdir(exist_ok=True)
Path("evidence/test-results.json").write_text(json.dumps({"passed": len(checks), "checks": checks},
    ensure_ascii=False, indent=2), encoding="utf-8")
print(f"PASS: {len(checks)} checks")
