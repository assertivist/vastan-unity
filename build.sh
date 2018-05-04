#!/bin/sh

project="vastan"
filename="$project"
unity_exe="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
options="-batchmode -nographics -silent-crashes -projectPath $(pwd)/$project"
workspace="$(pwd)/Build"
packages="$(pwd)/Package"
myhome="$(pwd)"
commit=`echo $TRAVIS_COMMIT | head -c 7`

mkdir $workspace
mkdir $packages

build_for_target()
{
  target=$1
  unitytarget=$2
  extension=$3

  echo "Attempting to build $project for $target"
  $unity_exe \
    $options \
    -logFile $(pwd)/Build/$target/build.log \
    -$unitytarget "$workspace/$target/$filename.$extension" \
    -quit
  success=$?
  cd $workspace
  zip -r9 $packages/$project-$target-$commit.zip $target
  
  cd $myhome
  return $success
}

build_for_target windows buildWindowsPlayer exe
winbuilt=$?
cat windows/build.log
build_for_target osx buildOSXUniversalPlayer app
macbuilt=$?
cat osx/build.log
build_for_target linux buildLinuxUniversalPlayer exe
linbuilt=$?
cat linux/build.log

exit $winbuilt || $macbuilt || $linbuilt

notify()
{
    (
        echo NICK vastanbuilds
        echo USER vastanbuilds 8 x : Notifier
        sleep 1
        echo 'JOIN #avaraline'
        echo 'PRIVMSG #avaraline' $1
        echo 'PART #avaraline'
        echo 'PRIVMSG #vastan' $1
        echo quit
    ) | nc avaraline.net 6667
}
