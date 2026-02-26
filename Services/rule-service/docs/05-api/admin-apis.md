# Admin APIs

Contract-first documentation for administrative endpoints.

## `/admin/rulesets`

### GET /admin/rulesets
Retrieve all rulesets.

#### Request
```http
GET /admin/rulesets
```

#### Response
```json
{
  "rulesets": []
}
```

---

### POST /admin/rulesets
Create a new ruleset.

#### Request
```http
POST /admin/rulesets
Content-Type: application/json
```

#### Response
```json
{
  "id": "",
  "name": "",
  "createdAt": ""
}
```

---

## `/admin/formulas`

### GET /admin/formulas
Retrieve all formulas.

#### Request
```http
GET /admin/formulas
```

#### Response
```json
{
  "formulas": []
}
```

---

### POST /admin/formulas
Create a new formula.

#### Request
```http
POST /admin/formulas
Content-Type: application/json
```

#### Response
```json
{
  "id": "",
  "name": "",
  "expression": "",
  "createdAt": ""
}
```
