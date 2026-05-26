#!/bin/bash

BASE_URL="http://localhost:5202"
DIVIDER="─────────────────────────────────────────────"

echo ""
echo "📚 BookLibrary API Benchmark"
echo "$DIVIDER"

# Helper — hits an endpoint and prints the response time
hit() {
    local method=$1
    local label=$2
    local url=$3
    local body=$4

    if [ -n "$body" ]; then
        response=$(curl -s -o /tmp/bench_response.json -w "%{http_code}" \
            -X "$method" "$url" \
            -H "Content-Type: application/json" \
            -H "Accept: application/json" \
            -d "$body" \
            -D /tmp/bench_headers.txt)
    else
        response=$(curl -s -o /tmp/bench_response.json -w "%{http_code}" \
            -X "$method" "$url" \
            -H "Accept: application/json" \
            -D /tmp/bench_headers.txt)
    fi

    time_ms=$(grep -i "x-response-time-ms" /tmp/bench_headers.txt | awk '{print $2}' | tr -d '\r')
    status=$response

    printf "%-8s %-35s  status: %s  time: %sms\n" "$method" "$label" "$status" "$time_ms"
}

# ── Seed a book so we have a known ID ──────────────────
echo ""
echo "▶ Seeding a test book..."
SEED_RESPONSE=$(curl -s -X POST "$BASE_URL/api/books" \
    -H "Content-Type: application/json" \
    -d '{"title":"Benchmark Book","author":"Test Author"}')

BOOK_ID=$(echo "$SEED_RESPONSE" | sed 's/.*"id":"\([^"]*\)".*/\1/')
echo "  Book ID: $BOOK_ID"
echo ""

# ── Run all endpoints ───────────────────────────────────
echo "▶ Running endpoints..."
echo ""

# ── Standard queries ───────────────────────────────────
echo "── Standard ──────────────────────────────────────────"
hit GET    "/api/books (list all 20k)"           "$BASE_URL/api/books"
hit GET    "/api/books/{id} (single lookup)"     "$BASE_URL/api/books/$BOOK_ID"
hit GET    "search?term=martin"                  "$BASE_URL/api/books/search?term=martin"
hit GET    "search?term=clean"                   "$BASE_URL/api/books/search?term=clean"
hit POST   "/api/books (create)"                 "$BASE_URL/api/books" '{"title":"New Book","author":"New Author"}'
hit PUT    "/api/books/{id} (update)"            "$BASE_URL/api/books/$BOOK_ID" '{"title":"Updated Book","author":"Updated Author"}'
hit DELETE "/api/books/{id} (delete)"            "$BASE_URL/api/books/$BOOK_ID"

echo ""

# ── Heavy queries ───────────────────────────────────────
echo "── Heavy ─────────────────────────────────────────────"
hit GET  "paged page=1 size=100"                 "$BASE_URL/api/books/paged?page=1&pageSize=100"
hit GET  "paged page=1 size=1000"                "$BASE_URL/api/books/paged?page=1&pageSize=1000"
hit GET  "stats/authors (GROUP BY + COUNT)"      "$BASE_URL/api/books/stats/authors"
hit GET  "by-author=Martin Fowler (full scan)"   "$BASE_URL/api/books/by-author?author=Martin%20Fowler"
hit GET  "by-author=Robert C. Martin"            "$BASE_URL/api/books/by-author?author=Robert%20C.%20Martin"
hit GET  "count (COUNT * full table)"            "$BASE_URL/api/books/count"

echo ""
echo "$DIVIDER"
echo "✅ Done"
echo ""
