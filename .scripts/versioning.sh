if [ ${GITHUB_REF_TYPE}  != "tag" ]; then
    exit 1
fi

version="${GITHUB_REF:11}"
git_hash="${GITHUB_SHA}"
echo $git_hash
git_hash=`expr substr $git_hash 1 8`

echo $version
echo $git_hash

sed -i "s/0.0.0/$version/g;s/development/$git_hash/g" src/Alma/Alma.csproj