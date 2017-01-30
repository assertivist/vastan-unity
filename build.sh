#!/bin/sh

project="vastan"
filename="$project"
unity_exe="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
options="-batchmode -nographics -silent-crashes -projectPath $(pwd)/$project"
workspace="$(pwd)/Build"
packages="$(pwd)/Package"
myhome="$(pwd)"
commit=`echo $TRAVIS_COMMIT | tail -c 7`

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

  cd $workspace
  zip -r9 $packages/$project-$target-$commit.zip $target
  cd $myhome
}

build_for_target windows buildWindowsPlayer exe
build_for_target osx buildOSXUniversalPlayer app
build_for_target linux buildLinuxUniversalPlayer exe

find $packages

