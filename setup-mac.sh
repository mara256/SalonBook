echo "================================================"
echo "  SalonBook — Setup Mac (SQLite)"
echo "================================================"

GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' 

echo ""
echo "▶ Verificare .NET SDK..."
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}✗ .NET SDK nu este instalat.${NC}"
    echo "  Instaleaza de la: https://dotnet.microsoft.com/download/dotnet/8.0"
    echo "  Sau cu Homebrew: brew install dotnet"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo -e "${GREEN}✓ .NET $DOTNET_VERSION detectat${NC}"

echo ""
echo "▶ Verificare dotnet-ef tools..."
if ! dotnet ef --version &> /dev/null; then
    echo -e "${YELLOW}  dotnet-ef nu este instalat. Se instaleaza...${NC}"
    dotnet tool install --global dotnet-ef
    export PATH="$PATH:$HOME/.dotnet/tools"
fi
echo -e "${GREEN}✓ dotnet-ef disponibil${NC}"

echo ""
echo "▶ Se descarca pachetele NuGet..."
dotnet restore
if [ $? -ne 0 ]; then
    echo -e "${RED}✗ Eroare la restore. Verifica conexiunea la internet.${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Pachete instalate${NC}"

echo ""
echo "▶ Build proiect..."
dotnet build --no-restore -q
if [ $? -ne 0 ]; then
    echo -e "${RED}✗ Eroare la build. Verifica codul.${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Build reusit${NC}"

echo ""
echo "▶ Creare baza de date SQLite..."
if [ ! -f "salonbook.db" ]; then
    dotnet ef migrations add InitialCreate --no-build 2>/dev/null || true
    dotnet ef database update --no-build
    if [ $? -ne 0 ]; then
        echo -e "${RED}✗ Eroare la crearea bazei de date.${NC}"
        exit 1
    fi
    echo -e "${GREEN}✓ Baza de date creata: salonbook.db${NC}"
else
    echo -e "${GREEN}✓ Baza de date existenta detectata${NC}"
    dotnet ef database update --no-build
fi

echo ""
echo "================================================"
echo -e "${GREEN}  Setup complet! Se porneste aplicatia...${NC}"
echo "  Deschide browserul la: http://localhost:5000"
echo ""
echo "  Cont admin implicit:"
echo "  Email:  admin@salonbook.ro"
echo "  Parola: Admin@1234"
echo "================================================"
echo ""

dotnet run
