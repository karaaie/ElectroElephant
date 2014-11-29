require 'bundler/setup'
require 'albacore'

Albacore::Tasks::Versionizer.new :versioning


desc 'restore all nugets as per the packages.config files'
nugets_restore :restore do |p|
  p.out = 'src/packages'
  p.exe = 'tools/NuGet.exe'
end

desc 'create assembly infos'
asmver_files :assembly_info => :versioning do |a|
  a.files = FileList['**/*proj'] # optional, will find all projects recursively by default

  # attributes are required:
  a.attributes assembly_description: 'ElectroElephant is a FSharp lib used to publish and subscribe events from Kafka.',
               assembly_configuration: Configuration,
               assembly_company: 'Kamil Hakim',
               assembly_copyright: "(c) #{Time.now.year} by Kamil Hakim",
               assembly_version: ENV['LONG_VERSION'],
               assembly_file_version: ENV['LONG_VERSION'],
               assembly_informational_version: ENV['BUILD_VERSION']
end

SOLUTION_FILE = 'src/ElectroElephant.sln'

desc 'Perform fast build (warn: doesn\'t d/l deps)'
build :quick_build do |b|
  b.sln = SOLUTION_FILE
end

desc 'perform full build'
build :build => [:versioning, :assembly_info, :restore] do |b|
  b.prop 'Configuration', Configuration
  b.sln = SOLUTION_FILE
end

build :clean_sln do |b|
  b.target = 'Clean'
  b.sln = SOLUTION_FILE
  b.prop 'Configuration', Configuration
end

desc 'clean'
task :clean => [:clean_sln] do
  FileUtils.rm_rf 'build'
end

task :test do
  Dir.glob("src/*.Tests/bin/#{Configuration}/*.Tests.exe").
  each do |exe|
    system exe, clr_command: true
  end
end
