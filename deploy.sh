#!/bin/sh

package_dir=$TRAVIS_BUILD_DIR/Package/
deploy_target="vastan@vastan.net:/home/vastan/public_html/release"
rsync_command=rsync -r --delete-after -e 'ssh -o StrictHostKeyChecking=no'

$rsync_command $package_dir $deploy_target/$1