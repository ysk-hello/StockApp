<template>
    <div>
        <table class="table">
            <thead class="sticky-top bg-white">
                <tr>
                    <th>コード</th>
                    <th>名前</th>
                    <th>RSI</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="c in companies">
                    <td>{{c.code}}</td>
                    <td><a :href="'https://kabutan.jp/stock/chart?code=' + c.code" target="_blank">{{c.name}}</a></td>
                    <td v-if="rsiList[c.code]">{{rsiList[c.code]['value']}}</td>
                    <td v-else></td>
                </tr>
            </tbody>
        </table>
    </div>
</template>

<script lang="ts">
    import { defineComponent } from 'vue';

    interface Company {
        code: string,
        name: string
    }

    interface Rsi {
        date: Date,
        value: number
    }

    export default defineComponent({
        data() {
            return {
                companies: [] as Company[],
                rsiList: {} as { [key: string]: Rsi }
            }
        },
        created() {
            this.getSelectedCompanies();
        },
        mounted() {
        },
        methods: {
            getSelectedCompanies() {
                this.companies = [];

                fetch('https://stockapp0610.azurewebsites.net/api/companies/selected')
                    .then(res => res.json())
                    .then((data: Array<Company>) => {
                        this.companies = data;
                    })
                    .then(() => {
                        this.getRsiList();
                    })
                    .catch(err => console.log(err));
            },
            getRsiList() {
                this.rsiList = {};

                this.companies.forEach(c => {
                    fetch('https://stockapp0610.azurewebsites.net/api/rsi/jpn/' + c.code)
                        .then(res => res.json())
                        .then((data: Rsi) => {
                            console.log(`${c.code} ${data.date} ${data.value}`);
                            this.rsiList[c.code] = data;
                        })
                        .catch(err => console.log(err));
                });
            }
        },
    });
</script>

<style>
</style>