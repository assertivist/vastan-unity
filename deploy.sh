#!/bin/sh

package_dir="$TRAVIS_BUILD_DIR/Package/"
deploy_dir="/home/vastan/public_html/release/$1"
deploy_host="vastan@vastan.net"
deploy_target="$deploy_host:$deploy_dir"
#scp -o StrictHostKeyChecking=no -r $TRAVIS_BUILD_DIR/Package/ vastan@vastan.net:/home/vastan/public_html/release/%{branch}
ssh -o StrictHostKeyChecking=no $deploy_host 'mkdir -p $deploy_dir'
rsync -r  -e 'ssh -o StrictHostKeyChecking=no' $package_dir $deploy_target