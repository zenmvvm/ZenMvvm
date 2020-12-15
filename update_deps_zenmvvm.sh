
RED='\033[0;31m' #${RED}
GREEN='\033[0;32m' #${GREEN}
CYAN='\033[0;36m' #${CYAN}
NC='\033[0m' # ${NC} -no color

exit_error()
{
    printf "${RED}Error. Stopping script!${NC}\n"; exit 1
}
#_____________________________________________________________

dep1="ZenMvvm.ZenIoc"
dep2="ZenMvvm.Autofac"
dep3="ZenMvvm.DryIoc"

prefix='Include="ZenMvvm" Version='

#_____________________________________________________________
printf "=%.0s" {1..80}; printf "\n"
printf "TODO: Github action using checkout"; printf "\n"
printf "=%.0s" {1..80}; printf "\n"

pwd
printf "=%.0s" {1..80}; printf "\n"

echo "Stashing, then pulling changes to develop from remote..."

cd "../${dep1}/"
git checkout develop
git add .
git stash save
git pull origin

cd "../${dep2}/"
git checkout develop
git add .
git stash save
git pull origin

cd "../${dep3}/"
git checkout develop
git add .
git stash save
git pull origin

echo ""
echo "Done. Any key to continue..."

printf "=%.0s" {1..80}; printf "\n"
echo "Checking for uncommitted changes"
cd "../${dep1}/"
pwd
if [ -z $(git status --porcelain) ]; then echo "ok";else exit_error; fi
cd "../${dep2}/"
pwd
if [ -z $(git status --porcelain) ]; then echo "ok";else exit_error; fi
cd "../${dep3}/"
pwd
if [ -z $(git status --porcelain) ]; then echo "ok";else exit_error; fi

echo "All clean. Any key to continue..."
read

echo "Existing version number (e.g \"0.9.0-pre\")? "
read existing
echo "Replacement? "
read replacement

cd ".."
echo "Replacing within files..."
find . \( -type d -name .git -prune \) -o -type f -name '*.csproj' -print0 | xargs -0 sed -i '' -e "s/$prefix\"$existing\"/$prefix\"$replacement\"/g"
echo "Success!! Any key to continue..."
read

printf "=%.0s" {1..80}; printf "\n"
cd "./${dep1}/"
git checkout develop
git add "./src/${dep1}/${dep1}.csproj"
git commit -m "Update ZenMvvm to version ${replacement}"	
printf "=%.0s" {1..80}; printf "\n"

printf "=%.0s" {1..80}; printf "\n"
cd "../${dep2}/"
git checkout develop
git add "./src/${dep2}/${dep2}.csproj"
git commit -m "Update ZenMvvm to version ${replacement}"	
printf "=%.0s" {1..80}; printf "\n"

printf "=%.0s" {1..80}; printf "\n"
cd "../${dep3}/"
git checkout develop
git add "./src/${dep3}/${dep3}.csproj"
git commit -m "Update ZenMvvm to version ${replacement}"	
printf "=%.0s" {1..80}; printf "\n"

while true; do
	read -p "Do you want to push changes?" yn
case $yn in
    [Yy]* ) break;;
    [Nn]* ) printf "Done!\n"; exit;;
    * ) echo "Please answer yes or no.";;
esac
done

cd "../${dep1}/"
git pull origin
git push origin develop

cd "../${dep2}/"
git pull origin
git push origin develop

cd "../${dep3}/"
git pull origin
git push origin develop



