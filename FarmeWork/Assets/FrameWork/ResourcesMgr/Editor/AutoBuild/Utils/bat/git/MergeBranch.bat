rem 合并分支

set formBranch=%1
set toBranch=%2
set toBranchIsExist=%3

rem 切到被合并的分支
git checkout %formBranch%
rem 丢弃暂存文件 与 以追踪的文件
git reset --hard
git pull

if %toBranchIsExist%==0 (
	rem 不存在就创建新分支
	git branch %toBranch%
	git checkout %toBranch%
	git push --set-upstream origin %toBranch%
)

rem 合并
git checkout %toBranch%
git pull
git merge %formBranch% --no-edit
git push